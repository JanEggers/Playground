using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;

namespace Playground.Core.Test;

public static class FancyScheduler
{
    public static IObservable<TSource> Schedule<TSource>(this IObservable<TSource> source)
    {
        return source.ScheduleBy(x => x);
    }
    public static IObservable<TSource> ScheduleBy<TSource, TKey>(this IObservable<TSource> source, Func<TSource, TKey> keyselector)
    {
        return source
        .GroupBy(keyselector)
        .SelectMany(partition => partition
        .ObserveOn(new EventLoopScheduler()));
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
        var scheduled = NumberGenerator.Generate(1, 1000)
        .ScheduleBy(x => x % 2 == 0);
        //.Schedule();

        var items = await scheduled
        .Select(i => new 
        {
            Value = i,
            Thread = Thread.CurrentThread.ManagedThreadId
        })
        .Take(take)
        .ToList();

        Assert.Equal(take, items.Count);

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
