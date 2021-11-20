using System;
using System.Reactive;
using System.Reactive.Linq;
using Serilog;

namespace EcsRx.Plugins.UnityUx {
    public sealed class UxContext {
        public UxContext Parent { get; }
        public IUxComponent Component { get; }
        public IObservable<Unit> Destroy { get; }
        public ILogger Logger { get; } // TODO figure out a way of doing logging without introducing a serilog dependency

        public UxContext(UxContext parent, IUxComponent component, IObservable<Unit> destroy, ILogger logger) {
            Parent = parent;
            Component = component;
            Destroy = destroy ?? throw new ArgumentNullException(nameof(destroy));
            Logger = logger ?? throw new ArgumentNullException(nameof(destroy));
        }

        public UxContext TakeUntil(IObservable<Unit> destroy) {
            if (destroy is null)
                throw new ArgumentNullException(nameof(destroy));
            return new UxContext(this, null, Destroy.Merge(destroy), Logger);
        }

        public T GetAncestor<T>() where T : IUxComponent {
            var current = this;
            do {
                current = current.Parent;
                if (current == null)
                    throw new ArgumentException($"No ancestor found using the component type '{typeof(T)}'", nameof(T));
            } while (!(current.Component is T));
            return (T)current.Component;
        }

        public static UxContext CreateRootContext(IObservable<Unit> destroy, ILogger logger) {
            return new UxContext(null, null, destroy, logger);
        }
    }
}
