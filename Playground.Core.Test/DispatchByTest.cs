using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using static Playground.Core.Test.DispatchByTest;

namespace Playground.Core.Test;

public static class FancyScheduler
{
    public static IObservable<TSource> Schedule<TSource>(this IObservable<TSource> source)
    {
        return new FancyScheduler<TSource, TSource>(source, x=> x);
    }
    public static IObservable<TSource> ScheduleBy<TSource, TKey>(this IObservable<TSource> source, Func<TSource, TKey> keyselector)
    {
        return new FancyScheduler<TSource, TKey>(source, keyselector);
    }
}

public class DispatchByTest
{
    public class NumberGenerator : IDisposable
    {
        private bool isDisposed;
        private NumberGenerator(IObserver<int> observer, int min, int max) 
        {
            Task.Run(() =>
            {
                var random = new Random();
                while (!isDisposed)
                {
                    observer.OnNext(random.Next(min, max));
                }
                observer.OnCompleted();
            });
        }

        public static IObservable<int> Generate(int min,int max) 
        {
            return Observable.Create<int>((observer) => new NumberGenerator(observer, min, max));
        }

        public void Dispose()
        {
            isDisposed = true;
        }
    }

    

    public class FancyScheduler<TSource, TKey> : IObservable<TSource>, IObserver<TSource>
    {
        private readonly IObservable<TSource> _source;
        private readonly Func<TSource, TKey> _keyselector;
        private ImmutableList<IObserver<TSource>> _observers = ImmutableList<IObserver<TSource>>.Empty;
        private ImmutableDictionary<TKey, Channel<TSource>> _buffers = ImmutableDictionary<TKey, Channel<TSource>>.Empty;

        public FancyScheduler(IObservable<TSource> source, Func<TSource, TKey> keyselector)
        {
            _source = source;
            _keyselector = keyselector;
        }

        public void OnCompleted()
        {
            foreach (var observer in _observers)
            {
                observer.OnCompleted();
            }
        }

        public void OnError(Exception error)
        {
            foreach (var observer in _observers)
            {
                observer.OnError(error);
            }
        }

        public void OnNext(TSource value)
        {
            var key = _keyselector(value);
            if (!_buffers.TryGetValue(key, out var buffer))
            {
                buffer = Channel.CreateUnbounded<TSource>();
                _buffers = _buffers.SetItem(key, buffer);

                Task.Factory.StartNew(() => RunBuffer(buffer.Reader), TaskCreationOptions.LongRunning);
            }

            buffer.Writer.TryWrite(value);           
        }

        private async Task RunBuffer(ChannelReader<TSource> reader) 
        {
            do
            {
                await reader.WaitToReadAsync();

                while (reader.TryRead(out var value))
                {
                    foreach (var observer in _observers)
                    {
                        observer.OnNext(value);
                    }
                }
            } while (!reader.Completion.IsCompleted);
        }

        public IDisposable Subscribe(IObserver<TSource> observer)
        {
            _observers = _observers.Add(observer);
            return _source.Finally(() => {
                _observers = _observers.Remove(observer);
                if (_observers.Count == 0)
                {
                    foreach (var buffer in _buffers)
                    {
                        buffer.Value.Writer.TryComplete();
                    }
                    _buffers = ImmutableDictionary<TKey, Channel<TSource>>.Empty;
                }
            }).Subscribe(this);
        }
    }

    [Fact]
    public async Task TestSchedule()
    {
        var scheduled = NumberGenerator.Generate(1, 5)
        .ScheduleBy(x => x % 2 == 0);
        //.Schedule();

        var x = await scheduled
        .Select(i => new 
        {
            Value = i,
            Thread = Thread.CurrentThread.ManagedThreadId
        })
        .Take(10)
        .ToList();

        await Task.Delay(1000);

    }

    [Fact]
    public async Task TestScheduleError()
    {
        var scheduled = NumberGenerator.Generate(1, 5)
        .Select(i => {
            throw new Exception("bäm");
            return i;
        })
        .Schedule();

        var x = await Assert.ThrowsAsync<Exception>(async () => await scheduled
        .Select(i => new
        {
            Value = i,
            Thread = Thread.CurrentThread.ManagedThreadId
        })
        .Take(10)
        .ToList());

        await Task.Delay(1000);

    }
}
