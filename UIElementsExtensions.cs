using System;
using System.Collections.Generic;
using System.Reactive;
using Rx.Data;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

namespace EcsRx.Plugins.UnityUx {
    // TODO find a way to have these methods use the Subscribe extension which passes a logger in, in a clean way!
    public static class UIElementsExtensions {
        public static void BindValue2Way2Enum<T>(this INotifyValueChanged<string> element, IReactiveProperty<T> boundProperty, IObservable<Unit> destroy, Func<T, bool> shouldBind = null) where T : struct, IConvertible {
            if (!typeof(T).IsEnum)
                throw new ArgumentException($"Supplied T {typeof(T)} is not an enum.", nameof(T));
            if (element is null)
                throw new ArgumentNullException(nameof(element));
            if (boundProperty is null)
                throw new ArgumentNullException(nameof(boundProperty));
            if (destroy is null)
                throw new ArgumentNullException(nameof(destroy));
            boundProperty.TakeUntil(destroy).Subscribe(value => {
                if (shouldBind == null || shouldBind(value))
                    element.SetValueWithoutNotify(value.ToString());
            });
            element.Change().TakeUntil(destroy).Subscribe(change => {
                if (Enum.TryParse(change.newValue, out T result) && (shouldBind == null || shouldBind(result)))
                    boundProperty.Value = result;
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BindValue2Way2Enum<T>(this INotifyValueChanged<string> element, IReactiveProperty<T> boundProperty, UxContext context, Func<T, bool> shouldBind = null) where T : struct, IConvertible {
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            BindValue2Way2Enum(element, boundProperty, context.Destroy, shouldBind);
        }

        public static void BindValue2Way<T>(this INotifyValueChanged<T> element, IReactiveProperty<T> boundProperty, IObservable<Unit> destroy, Func<T, bool> shouldBind = null) {
            if (element is null)
                throw new ArgumentNullException(nameof(element));
            if (boundProperty is null)
                throw new ArgumentNullException(nameof(boundProperty));
            if (destroy is null)
                throw new ArgumentNullException(nameof(destroy));
            var boundObservable = shouldBind != null ? boundProperty.Where(shouldBind) : boundProperty;
            boundObservable.TakeUntil(destroy).Subscribe(element.SetValueWithoutNotify);
            element.Change().TakeUntil(destroy).Subscribe(change => boundProperty.Value = change.newValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BindValue2Way<T>(this INotifyValueChanged<T> element, IReactiveProperty<T> boundProperty, UxContext context, Func<T, bool> shouldBind = null) {
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            BindValue2Way(element, boundProperty, context.Destroy, shouldBind);
        }

        public static void BindValue<T>(this INotifyValueChanged<T> element, IObservable<T> boundObservable) {
            if (element is null)
                throw new ArgumentNullException(nameof(element));
            if (boundObservable is null)
                throw new ArgumentNullException(nameof(boundObservable));
            boundObservable.Subscribe(element.SetValueWithoutNotify);
        }

        public static void BindText(this TextElement textElement, IObservable<string> textObservable) {
            if (textElement is null)
                throw new ArgumentNullException(nameof(textElement));
            if (textObservable is null)
                throw new ArgumentNullException(nameof(textObservable));
            textObservable.Subscribe(text => textElement.text = text);
        }

        public static void BindEnableInClassList(this VisualElement source, string className, IObservable<bool> enableObservable) {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (enableObservable is null)
                throw new ArgumentNullException(nameof(enableObservable));
            enableObservable.Subscribe(enable => source.EnableInClassList(className, enable));
        }

        public static void BindEnabled(this VisualElement source, IObservable<bool> enabledObservable) {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (enabledObservable is null)
                throw new ArgumentNullException(nameof(enabledObservable));
            enabledObservable.DistinctUntilChanged().Subscribe(enable => source.SetEnabled(enable));
        }

        public static void BindDisplayed(this VisualElement source, IObservable<bool> displayedObservable) {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (displayedObservable is null)
                throw new ArgumentNullException(nameof(displayedObservable));
            displayedObservable.DistinctUntilChanged().Subscribe(enable => source.SetDisplayed(enable));
        }

        public static void SetDisplayed(this VisualElement source, bool displayed) {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            source.style.display = new StyleEnum<DisplayStyle>(displayed ? DisplayStyle.Flex : DisplayStyle.None);
        }

        public static void ClearChildren(this VisualElement source) {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            for (var i = source.childCount - 1; i >= 0; i--)
                source.RemoveAt(i);
        }

        public static void AddRange(this VisualElement source, IEnumerable<VisualElement> toAdd) {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (toAdd is null)
                throw new ArgumentNullException(nameof(toAdd));
            foreach (var item in toAdd)
                source.Add(item);
        }

        public static void InsertRange(this VisualElement source, int index, IEnumerable<VisualElement> toAdd) {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (toAdd is null)
                throw new ArgumentNullException(nameof(toAdd));
            foreach (var item in toAdd)
                source.Insert(index++, item);
        }

        public static IObservable<TEventType> On<TEventType>(this VisualElement source) where TEventType : EventBase<TEventType>, new() {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            return new EventObservable<TEventType>(source);
        }

        public static IObservable<ChangeEvent<T>> Change<T>(this INotifyValueChanged<T> source) {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            return new ChangeEventObservable<T>(source);
        }

        public static IObservable<ClickEvent> Click(this VisualElement source) {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            return new EventObservable<ClickEvent>(source);
        }

        public static IObservable<bool> Hover(this VisualElement source) {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            return new HoverObservable(source);
        }

        public static IUxComponent ToUxComponent(this VisualElement source) {
            if (source is null) throw new ArgumentNullException(nameof(source));
            return new StaticComponent(source);
        }

        private class ChangeEventObservable<T> : IObservable<ChangeEvent<T>> {
            private readonly INotifyValueChanged<T> _source;
            private Subject<ChangeEvent<T>> _subject;

            public ChangeEventObservable(INotifyValueChanged<T> source) {
                _source = source;
            }

            public IDisposable Subscribe(IObserver<ChangeEvent<T>> observer) {
                if (observer is null)
                    throw new ArgumentNullException(nameof(observer));
                if (_subject != null)
                    throw new InvalidOperationException("Only is only supported once!");
                _subject = new Subject<ChangeEvent<T>>();
                _source.RegisterValueChangedCallback(_subject.OnNext);
                _subject.Finally(OnFinally);
                _subject.Subscribe(observer);
                return _subject;
            }

            private void OnFinally() => _source.RegisterValueChangedCallback(_subject.OnNext);
        }

        private class EventObservable<TEventType> : IObservable<TEventType> where TEventType : EventBase<TEventType>, new() {
            private readonly VisualElement _source;
            private Subject<TEventType> _subject;

            public EventObservable(VisualElement source) {
                _source = source;
            }

            public IDisposable Subscribe(IObserver<TEventType> observer) {
                if (observer is null)
                    throw new ArgumentNullException(nameof(observer));
                if (_subject != null)
                    throw new InvalidOperationException("Only is only supported once!");
                _subject = new Subject<TEventType>();
                _source.RegisterCallback<TEventType>(_subject.OnNext);
                _subject.Finally(OnFinally);
                _subject.Subscribe(observer);
                return _subject;
            }

            private void OnFinally() => _source.UnregisterCallback<TEventType>(_subject.OnNext);
        }

        private class HoverObservable : IObservable<bool> {
            private readonly VisualElement _source;
            private BehaviorSubject<bool> _hover;

            public HoverObservable(VisualElement source) {
                _source = source;
            }

            public IDisposable Subscribe(IObserver<bool> observer) {
                if (observer is null)
                    throw new ArgumentNullException(nameof(observer));
                if (_hover != null)
                    throw new InvalidOperationException("Only is only supported once!");
                _hover = new BehaviorSubject<bool>(false);
                _source.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
                _source.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
                _hover.Finally(OnFinally);
                _hover.Subscribe(observer);
                return _hover;
            }

            private void OnFinally() {
                _source.UnregisterCallback<MouseEnterEvent>(OnMouseEnter);
                _source.UnregisterCallback<MouseLeaveEvent>(OnMouseLeave);
            }

            private void OnMouseEnter(MouseEnterEvent evt) => _hover.OnNext(true);

            private void OnMouseLeave(MouseLeaveEvent evt) => _hover.OnNext(false);
        }
    }
}
