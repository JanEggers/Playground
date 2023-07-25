using System;
using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;

namespace Playground.Core.Test;

public static class FancyScheduler
{
    public static IObservable<TSource> ScheduleBy<TSource, TKey>(this IObservable<TSource> source, Func<TSource, TKey> keyselector, Action<TSource> work)
    {
        return source
        .GroupBy(keyselector)
        .SelectMany(partition => Observable.Using(() => new EventLoopScheduler(), scheduler => partition
            .ObserveOn(scheduler).Do(work)));
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

    [Fact]
    public async Task TestSchedule()
    {
        var take = 100000;

        var items = new BlockingCollection<object>();

        NumberGenerator.Generate(1, 1000)
        .ScheduleBy(x => x % Environment.ProcessorCount, i =>
        {
            var str = "";
            for (int x = 0; x < 10; x++)
            {
                str += x.ToString();
            }
            items.Add(new
            {
                Value = i,
                Thread = Thread.CurrentThread.ManagedThreadId
            });
        })

        //.ScheduleBy(x => x % Environment.ProcessorCount, i =>
        //return new
        //{
        //    Value = i,
        //    Thread = Thread.CurrentThread.ManagedThreadId
        //})
        //.Take(take / Environment.ProcessorCount)
        //.Do(i => items.Add(i))

        .Subscribe();
        ;

        SpinWait.SpinUntil(() => items.Count >= take);

        await Task.Delay(1000);

    }

    //[Fact]
    //public async Task TestScheduleError()
    //{
    //    var scheduled = NumberGenerator.Generate(1, 5)
    //    .Select(i => {
    //        throw new Exception("bäm");
    //        return i;
    //    })
    //    .Schedule();

    //    var x = await Assert.ThrowsAsync<Exception>(async () => await scheduled
    //    .Select(i => new
    //    {
    //        Value = i,
    //        Thread = Thread.CurrentThread.ManagedThreadId
    //    })
    //    .Take(10)
    //    .ToList());

    //    await Task.Delay(1000);

    //}
}
