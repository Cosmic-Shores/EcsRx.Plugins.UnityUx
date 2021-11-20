using UnityEngine.UIElements;

namespace EcsRx.Plugins.UnityUx {
    sealed class StaticComponent : IUxComponent {
        public VisualElement Content { get; }

        public StaticComponent(VisualElement content) {
            Content = content;
        }
    }
}
