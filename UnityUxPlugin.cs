using System;
using System.Collections.Generic;
using SystemsRx.Executor.Handlers;
using SystemsRx.Infrastructure.Dependencies;
using SystemsRx.Infrastructure.Plugins;
using SystemsRx.Infrastructure.Extensions;
using SystemsRx.Systems;

namespace EcsRx.Plugins.UnityUx {
    public sealed class UnityUxPlugin : ISystemsRxPlugin {
        public string Name => "Unity Ux Plugin";
        public Version Version { get; } = new Version("1.0.0");

        public void SetupDependencies(IDependencyContainer container) {
            container.Bind<IUxBindingStore, UxBindingStore>(x => x.AsSingleton());
            container.Bind<IUxBinderProvider>(x => x.ToMethod(c => c.Resolve<IUxBindingStore>()));
            container.Bind<IConventionalSystemHandler, UxBindingRegistrationHandler>();
            container.Bind<IUxBindingService, UxBindingService>(x => x.AsSingleton());
        }

        public IEnumerable<ISystem> GetSystemsForRegistration(IDependencyContainer container) => new ISystem[0];
    }
}
