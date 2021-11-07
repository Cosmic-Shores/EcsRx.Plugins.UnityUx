using System;
using System.Collections.Generic;
using System.Linq;
using SystemsRx.Attributes;
using SystemsRx.Executor.Handlers;
using SystemsRx.Extensions;
using SystemsRx.Systems;

namespace EcsRx.Plugins.UnityUx {
    [Priority(25)]
    sealed class UxBindingRegistrationHandler : IConventionalSystemHandler {
        private readonly IUxBindingStore _uxBindingStore;

        public UxBindingRegistrationHandler(IUxBindingStore uxBindingStore) {
            _uxBindingStore = uxBindingStore;
        }

        public bool CanHandleSystem(ISystem system) => system.MatchesSystemTypeWithGeneric(typeof(IUxBinder<>));

        public void SetupSystem(ISystem system) {
            foreach (var componentType in GetComponentTypes(system)) {
                if (_uxBindingStore.BinderByComponentType.TryGetValue(componentType, out var existingSystem))
                    throw new InvalidOperationException($"SetupSystem: Systems '{system.GetType()}' and '{existingSystem.GetType()}' are both trying to handle bindings for the component type '{componentType}'."
                        + Environment.NewLine + "Please ensure there's only one system trying to do that.");
                _uxBindingStore.BinderByComponentType.Add(componentType, system);
            }
        }

        public void DestroySystem(ISystem system) {
            foreach (var componentType in GetComponentTypes(system))
                _uxBindingStore.BinderByComponentType.Remove(componentType);
        }

        public void Dispose() { }

        private IEnumerable<Type> GetComponentTypes(ISystem system) {
            return system.GetGenericInterfacesFor(typeof(IUxBinder<>)).Select(interfaceType => interfaceType.GetGenericArguments()[0]);
        }
    }
}
