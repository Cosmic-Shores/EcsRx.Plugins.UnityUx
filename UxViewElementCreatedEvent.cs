using UnityEngine.UIElements;

namespace EcsRx.Plugins.UnityUx {
    public struct UxViewElementCreatedEvent {
        public VisualElement Container { get; private set; }

        public UxViewElementCreatedEvent(VisualElement container) {
            Container = container;
        }
    }
}
