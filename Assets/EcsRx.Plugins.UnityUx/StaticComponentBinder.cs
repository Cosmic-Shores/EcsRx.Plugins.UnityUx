using UnityEngine.UIElements;

namespace EcsRx.Plugins.UnityUx {
    sealed class StaticComponentBinder : IUxBinder<StaticComponent> {
        public VisualElement CreateBoundView(StaticComponent staticComponent, UxContext context) => staticComponent.Content;
    }
}
