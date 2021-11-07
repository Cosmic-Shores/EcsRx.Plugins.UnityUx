using System;
using System.Diagnostics;
using System.Reactive;
using Rx.Data.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace EcsRx.Plugins.UnityUx {
    public static class ObservableExtensions {
        public static IObservable<T> TakeUntil<T>(this IObservable<T> source, UxContext context) {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            return source.TakeUntil(context.Destroy);
        }

        /// <summary>
        /// Publishes all changes to the target but never complete the target.
        /// The created connection is disposed when context.Destroy emits. This methods also logs.
        /// </summary>
        public static void BindTo<T>(this IObservable<T> source, ISubject<T> target, UxContext context) {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (target is null)
                throw new ArgumentNullException(nameof(target));
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            source.TakeUntil(context.Destroy).Subscribe(new BoundPublisher<T>(target, context));
        }

        /// <summary>
        /// Publishes all changes to the target but never complete the target.
        /// The created connection is disposed when context.Destroy emits. This methods also logs.
        /// </summary>
        public static void BindTo<T>(this IObservable<T> source, ISubject<Unit> target, UxContext context) {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (target is null)
                throw new ArgumentNullException(nameof(target));
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            source.TakeUntil(context.Destroy).AsUnitObservable().Subscribe(new BoundPublisher<Unit>(target, context));
        }

        /// <summary>
        /// Publishes all changes to the target but never complete the target.
        /// </summary>
        public static void BindTo<T>(this IObservable<T> source, ISubject<T> target) {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            source.Subscribe(target.OnNext, target.OnError);
        }

        /// <summary>
        /// Publishes all changes to the target but never complete the target.
        /// </summary>
        public static void BindTo<T>(this IObservable<T> source, ISubject<Unit> target) {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            source.AsUnitObservable().Subscribe(target.OnNext, target.OnError);
        }

        private struct BoundPublisher<T> : IObserver<T> {
            private readonly ISubject<T> _target;
            private readonly UxContext _context;

            public BoundPublisher(ISubject<T> target, UxContext context) {
                _target = target;
                _context = context;
            }

            public void OnCompleted() => LogCompleted();

            public void OnError(Exception error) {
                _context.Logger.Warning(error, $"{nameof(BoundPublisher<T>)}: The source emited an error. {{ChangeType}}", typeof(T).Name);
                try {
                    _target.OnError(error);
                }
                catch (Exception ex) {
                    _context.Logger.Error(ex, $"{nameof(BoundPublisher<T>)}: An unhandled error occured publishing sources error. {{ChangeType}}", typeof(T).Name);
                }
            }

            public void OnNext(T value) {
                try {
                    _target.OnNext(value);
                }
                catch (Exception ex) {
                    _context.Logger.Error(ex, $"{nameof(BoundPublisher<T>)}: An unhandled error occured publishing change {{ChangeType}}", typeof(T).Name);
                }
            }

            [Conditional("DEBUG")]
            private void LogCompleted() => _context.Logger.Verbose($"{nameof(BoundPublisher<T>)} with {{ChangeType}} has completed", typeof(T).Name);

            [Conditional("DEBUG")]
            private void LogNext() => _context.Logger.Verbose($"{nameof(BoundPublisher<T>)}: propagating emited change {{ChangeType}}", typeof(T).Name);
        }
    }
}
