using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using UnityEngine.UIElements;

namespace EcsRx.Plugins.UnityUx {
    // VisualElement is currently the most barebone way to make your control implement CallbackEventHandler due to DispatchMode being marked internal.
    // INotifyValueChangedExtensions.RegisterValueChangedCallback() will only work as expected when the control implements CallbackEventHandler.
    /// <summary>
    /// This control is intended to be used with a 2 way binding otherwise the DropdownField and TextField won't actually be syncronized.
    /// </summary>
    public sealed class DropdownOrLabelField : VisualElement, INotifyValueChanged<string> {
        private static readonly Func<string, string> _passValue = x => x;
        private readonly Func<string, string> _formatSelectedValueCallback;
        private readonly Func<string, string> _formatListItemCallback;

        /// <summary>
        /// Might be null if an empty list of choices was supplied
        /// </summary>
        public DropdownField DropdownField { get; }
        public TextField TextField { get; }

        /// <param name="choices">If there are no choices the field will never become editable.</param>
        public DropdownOrLabelField(string label, IObservable<bool> editable, List<string> choices, IObservable<Unit> destroy, IObservable<bool> displayed = null, Func<string, string> formatSelectedValueCallback = null, Func<string, string> formatListItemCallback = null) {
            if (editable is null)
                throw new ArgumentNullException(nameof(editable));
            if (choices is null)
                throw new ArgumentNullException(nameof(choices));
            var hasChoices = choices.Any();
            if (hasChoices) {
                DropdownField = new DropdownField(label, choices, 0, formatSelectedValueCallback, formatListItemCallback);
                DropdownField.Change().TakeUntil(destroy).Subscribe(HandleEvent);
                DropdownField.BindDisplayed(displayed != null ? displayed.CombineLatest(editable, (isDisplayed, isEditable) => isDisplayed && isEditable) : editable);
            }
            TextField = new TextField(label);
            TextField.SetEnabled(false);
            var textFieldDisplayed = hasChoices
                ? (displayed != null ? displayed.CombineLatest(editable, (isDisplayed, isEditable) => isDisplayed && !isEditable) : editable.Select(x => !x))
                : displayed;
            TextField.BindDisplayed(textFieldDisplayed);
            _formatSelectedValueCallback = formatSelectedValueCallback ?? _passValue;
            _formatListItemCallback = formatListItemCallback ?? _passValue;
        }

        public IEnumerable<VisualElement> Elements {
            get {
                if (DropdownField != null)
                    yield return DropdownField;
                yield return TextField;
            }
        }

        public string value {
            get {
                return DropdownField != null
                    ? DropdownField.value
                    : _formatListItemCallback(TextField.value);
            }
            set {
                if (DropdownField != null)
                    DropdownField.value = value;
                TextField.value = _formatSelectedValueCallback(value);
            }
        }

        public void SetValueWithoutNotify(string newValue) {
            if (DropdownField != null)
                DropdownField.SetValueWithoutNotify(newValue);
            TextField.SetValueWithoutNotify(_formatSelectedValueCallback(value));
        }
    }
}
