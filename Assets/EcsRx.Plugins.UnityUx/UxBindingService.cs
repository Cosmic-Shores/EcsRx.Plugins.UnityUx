using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using SystemsRx.Events;
using UniRx;
using UnityEngine;
using UnityEngine.UIElements;

namespace EcsRx.Plugins.UnityUx {
    // TODO find a way to use the Subscribe extension which passes a logger in, in a clean way!
    sealed class UxBindingService : IUxBindingService {
        private readonly IUxBinderProvider _uxBinderProvider;
        private readonly IEventSystem _eventSystem;
        private readonly Serilog.ILogger _logger; // TODO figure out a way of doing logging without introducing a serilog dependency
        private readonly MethodInfo _createGenericBoundViewMethod;

        public UxBindingService(IUxBinderProvider uxBinderProvider, IEventSystem eventSystem, Serilog.ILogger logger) {
            _uxBinderProvider = uxBinderProvider;
            _eventSystem = eventSystem;
            _logger = logger.ForContext<UxBindingService>();
            _createGenericBoundViewMethod = typeof(UxBindingService).GetMethod(nameof(CreateGenericBoundView), BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public void PopulateChild(UxContext context, VisualElement container, IUxComponent component) {
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            if (container is null)
                throw new ArgumentNullException(nameof(container));
            var child = CreateBoundView(context, component, context.Destroy);
            container.Add(child);
            context.Destroy.Take(1).Subscribe(_ => container.Remove(child));
        }

        public void PopulateChild(UxContext context, VisualElement container, IObservable<IUxComponent> modelObservable) {
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            if (container is null)
                throw new ArgumentNullException(nameof(container));
            if (modelObservable is null)
                throw new ArgumentNullException(nameof(modelObservable));
            PopulateChildInternal(context, container, modelObservable);
        }

        public void PopulateChild<T>(UxContext context, VisualElement container, IObservable<T> modelObservable) where T : IUxComponent {
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            if (container is null)
                throw new ArgumentNullException(nameof(container));
            if (modelObservable is null)
                throw new ArgumentNullException(nameof(modelObservable));
            PopulateChildInternal(context, container, modelObservable);
        }

        public void PopulateChildren(UxContext context, VisualElement container, IEnumerable<IUxComponent> models) {
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            if (container is null)
                throw new ArgumentNullException(nameof(container));
            if (models is null)
                throw new ArgumentNullException(nameof(models));
            PopulateChildrenInternal(context, container, models);
        }

        public void PopulateChildren<T>(UxContext context, VisualElement container, IEnumerable<T> models) where T : IUxComponent {
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            if (container is null)
                throw new ArgumentNullException(nameof(container));
            if (models is null)
                throw new ArgumentNullException(nameof(models));
            PopulateChildrenInternal(context, container, models);
        }

        public void PopulateChildren(UxContext context, VisualElement container, IReadOnlyReactiveCollection<IUxComponent> components) {
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            if (container is null)
                throw new ArgumentNullException(nameof(container));
            if (components is null)
                throw new ArgumentNullException(nameof(components));
            PopulateChildrenInternal(context, container, components);
        }

        public void PopulateChildren<T>(UxContext context, VisualElement container, IReadOnlyReactiveCollection<T> components) where T : IUxComponent {
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            if (container is null)
                throw new ArgumentNullException(nameof(container));
            if (components is null)
                throw new ArgumentNullException(nameof(components));
            PopulateChildrenInternal(context, container, components);
        }

        private void PopulateChildInternal<T>(UxContext context, VisualElement container, IObservable<T> modelObservable) {
            var destroy = context.Destroy;
            var innerDestroyer = new Subject<Unit>();

            modelObservable.TakeUntil(destroy).Subscribe(component => {
                innerDestroyer.OnNext(Unit.Default);
                var child = CreateBoundView(context, component, innerDestroyer.Take(1));
                container.Add(child);
                innerDestroyer.Take(1).Subscribe(_ => container.Remove(child));
            });
            destroy.Take(1).Subscribe(innerDestroyer.OnNext);
        }

        private void PopulateChildrenInternal<T>(UxContext context, VisualElement container, IEnumerable<T> models) {
            var destroy = context.Destroy;
            var innerDestroyer = new Subject<Unit>();

            var children = models.Select(model => CreateBoundView(context, model, context.Destroy)).ToArray();
            container.AddRange(children);
            context.Destroy.Take(1).Subscribe(_ => {
                foreach (var child in children)
                    container.Remove(child);
            });
        }

        // TOOD maybe use https://github.com/reactivemarbles/DynamicData here instead of IReadOnlyReactiveCollection
        private void PopulateChildrenInternal<T>(UxContext context, VisualElement container, IReadOnlyReactiveCollection<T> components) {
            _ = new UxComponentReactiveCollectionBinder<T>(this, context, container, components);
        }

        private VisualElement CreateBoundView<T>(UxContext parent, T component, IObservable<Unit> destroy) {
            var toReturn = (VisualElement)_createGenericBoundViewMethod.MakeGenericMethod(component.GetType()).Invoke(this, new object[] { parent, component, destroy });
            _eventSystem.Publish(new UxViewElementCreatedEvent(toReturn));
            return toReturn;
        }

        private VisualElement CreateGenericBoundView<T>(UxContext parent, T component, IObservable<Unit> destroy) where T : IUxComponent {
            try {
                var binder = _uxBinderProvider.GetBinder<T>();
                var context = new UxContext(parent, component, destroy, parent.Logger.ForContext(binder.GetType()));
                return binder.CreateBoundView(component, context);
            }
            catch (Exception ex) {
                _logger.Error(ex, "Error creating a bound view for {UxComponent}", typeof(T));
                var label = new Label { text = $"Error creating a bound view for {typeof(T)}!{Environment.NewLine}{ex}" };
                label.style.backgroundColor = new StyleColor(Color.black);
                label.style.color = new StyleColor(new Color(255, 128, 128));
                return label;
            }
        }

        private class UxComponentReactiveCollectionBinder<T> {
            private readonly List<ISubject<Unit>> _destroyByIndex = new List<ISubject<Unit>>();
            private readonly UxBindingService _bindingService;
            private readonly UxContext _context;
            private readonly VisualElement _container;
            private readonly int _indexOffset;
            private readonly Serilog.ILogger _logger; // TODO figure out a way of doing logging without introducing a serilog dependency

            public UxComponentReactiveCollectionBinder(UxBindingService bindingService, UxContext context, VisualElement container, IReadOnlyReactiveCollection<T> components) {
                _bindingService = bindingService;
                _context = context;
                _container = container;
                _indexOffset = container.childCount;
                _logger = context.Logger
#if DEBUG
                    .ForContext("{UxComponentReactiveCollectionBinderInstance}", Guid.NewGuid())
#endif
                    .ForContext("UxComponentType", typeof(T));

                var destroy = context.Destroy;
                components.Select((component, i) => new CollectionAddEvent<T>(i, component)).ToObservable()
                    .Concat(components.ObserveAdd())
                    .TakeUntil(destroy)
                    .Subscribe(OnAdded);
                components.ObserveRemove().TakeUntil(destroy).Subscribe(OnRemoved);
                components.ObserveMove().TakeUntil(destroy).Subscribe(OnMoved);
                components.ObserveReplace().TakeUntil(destroy).Subscribe(OnReplaced);
                components.ObserveReset().TakeUntil(destroy).Merge(destroy.Take(1)).Subscribe(OnReset);
            }

            private void OnAdded(CollectionAddEvent<T> added) {
                LogChange(nameof(OnAdded), added);
                try {
                    if (IsChildCountInvalid())
                        return;
                    var destroy = new Subject<Unit>();
                    _destroyByIndex.Insert(added.Index, destroy);
                    var child = _bindingService.CreateBoundView(_context, added.Value, destroy.Take(1));
                    _container.Insert(_indexOffset + added.Index, child);
                }
                catch (Exception ex) {
                    LogChangeError(ex, nameof(OnAdded), added);
                }
            }

            private void OnRemoved(CollectionRemoveEvent<T> removed) {
                LogChange(nameof(OnRemoved), removed);
                try {
                    if (IsChildCountInvalid())
                        return;
                    _container.RemoveAt(_indexOffset + removed.Index);
                    _destroyByIndex[removed.Index].OnNext(Unit.Default);
                    _destroyByIndex.RemoveAt(removed.Index);
                }
                catch (Exception ex) {
                    LogChangeError(ex, nameof(OnRemoved), removed);
                }
            }

            private void OnMoved(CollectionMoveEvent<T> movement) {
                LogChange(nameof(OnMoved), movement);
                try {
                    if (IsChildCountInvalid())
                        return;
                    var oldContainerIndex = _indexOffset + movement.OldIndex;
                    var child = _container[oldContainerIndex];
                    _container.RemoveAt(oldContainerIndex);
                    _container.Insert(_indexOffset + movement.NewIndex, child);

                    var destroy = _destroyByIndex[movement.OldIndex];
                    _destroyByIndex.RemoveAt(movement.OldIndex);
                    _destroyByIndex.Insert(movement.NewIndex, destroy);
                }
                catch (Exception ex) {
                    LogChangeError(ex, nameof(OnMoved), movement);
                }
            }

            private void OnReplaced(CollectionReplaceEvent<T> replacement) {
                LogChange(nameof(OnReplaced), replacement);
                try {
                    if (IsChildCountInvalid())
                        return;
                    var containerIndex = _indexOffset + replacement.Index;
                    _container.RemoveAt(containerIndex);
                    var destroy = _destroyByIndex[replacement.Index];
                    destroy.OnNext(Unit.Default);
                    var child = _bindingService.CreateBoundView(_context, replacement.NewValue, destroy.Take(1));
                    _container.Insert(containerIndex, child);
                }
                catch (Exception ex) {
                    LogChangeError(ex, nameof(OnReplaced), replacement);
                }
            }

            private void OnReset(Unit unit) {
                LogChange(nameof(OnReset), unit);
                try {
                    if (IsChildCountInvalid())
                        return;
                    for (var i = _indexOffset + _destroyByIndex.Count - 1; i >= _indexOffset; i--)
                        _container.RemoveAt(i);
                    foreach (var innerDestroy in _destroyByIndex)
                        innerDestroy.OnNext(Unit.Default);
                    _destroyByIndex.Clear();
                }
                catch (Exception ex) {
                    LogChangeError(ex, nameof(OnReset), unit);
                }
            }

            private bool IsChildCountInvalid() {
                var expectedMinimumChildCount = _indexOffset + _destroyByIndex.Count;
                var childCount = _container.childCount;
                var toReturn = expectedMinimumChildCount > childCount;
                if (toReturn) {
                    var complaint = "Fewer children present than expected!";
                    var message = $"The Children on an element which was has an {nameof(IReadOnlyReactiveCollection<T>)} bound to it were modified in a way which isn't supported." + Environment.NewLine +
                        "Once the binding was created the count of children that are unrelated to that binding may not be changed. Unless they are appended at the end. Never remove bound chilren externally.";
                    _logger.Error($"{complaint} {{ExpectedMinimumChildCount}} {{PresentChildCount}} {message}", expectedMinimumChildCount, childCount);
                }
                return toReturn;
            }

            [Conditional("DEBUG")]
            private void LogChange(string methodName, object change) {
                _logger.Verbose($"{nameof(UxComponentReactiveCollectionBinder<T>)}.{methodName} {{@UxComponentChange}}", change);
            }

            private void LogChangeError(Exception exception, string methodName, object change) {
                _logger.Error(exception, $"{nameof(UxComponentReactiveCollectionBinder<T>)}.{methodName} failed! {{@UxComponentChange}}", change);
            }
        }
    }
}
