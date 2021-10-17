using System;
using System.Collections.Generic;
using SystemsRx.Systems;

namespace EcsRx.Plugins.UnityUx {
    interface IUxBindingStore : IUxBinderProvider {
        IDictionary<Type, ISystem> BinderByComponentType { get; }
    }
}
