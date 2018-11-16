using System;
using System.Reactive;

namespace NLightning.Utils.Extensions
{
    public static class ObservableExtensions
    {
        public static IDisposable CatchedSubscribe<T, TException>(this IObservable<T> source, Action<T> onNext, Action<T, TException> onException) where TException : Exception
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (onNext == null)
            {
                throw new ArgumentNullException(nameof(onNext));
            }

            Action Nop = () => {};
            Action<Exception> ErrorNop = ex => {};

            var onNextCatched = new Action<T>(obj =>
            {
                try
                {
                    onNext(obj);
                }
                catch (TException exception)
                {
                    onException(obj, exception);
                }
            });
            
            return source.Subscribe(new AnonymousObserver<T>(onNextCatched, ErrorNop, Nop));
        }
    }
}