using SystemsRx.Systems;
using UnityEngine.UIElements;

namespace EcsRx.Plugins.UnityUx {
    public interface IUxBinder<T> : ISystem where T : IUxComponent {
        VisualElement CreateBoundView(T component, UxContext context);
    }
}
