using System;
using System.Collections.Generic;
using Rx.Data;
using UnityEngine.UIElements;

namespace EcsRx.Plugins.UnityUx {
    public interface IUxBindingService {
        void PopulateChild(UxContext context, VisualElement container, IUxComponent component);
        void PopulateChild(UxContext context, VisualElement container, IObservable<IUxComponent> componentObservable);
        void PopulateChild<T>(UxContext context, VisualElement container, IObservable<T> modelObservable) where T : IUxComponent;
        void PopulateChildren(UxContext context, VisualElement container, IEnumerable<IUxComponent> models);
        void PopulateChildren<T>(UxContext context, VisualElement container, IEnumerable<T> models) where T : IUxComponent;
        void PopulateChildren(UxContext context, VisualElement container, IReadOnlyReactiveCollection<IUxComponent> components);
        void PopulateChildren<T>(UxContext context, VisualElement container, IReadOnlyReactiveCollection<T> components) where T : IUxComponent;
    }
}
