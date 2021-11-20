using System;
using System.Collections.Generic;
using SystemsRx.Systems;

namespace EcsRx.Plugins.UnityUx {
    sealed class UxBindingStore : IUxBindingStore {
        public IDictionary<Type, ISystem> BinderByComponentType { get; } = new Dictionary<Type, ISystem>();

        public IUxBinder<T> GetBinder<T>() where T : IUxComponent {
            var componentType = typeof(T);
            if (BinderByComponentType.TryGetValue(componentType, out var system))
                return (IUxBinder<T>)system;

            throw new ArgumentException($"No binder was registered to handle component type '{typeof(T)}'.", nameof(T));
        }
    }
}
