using System;
using R3;
using TMPro;
using UnityEngine.UI;

namespace CustomUtils.Runtime.Extensions.Observables
{
    /// <summary>
    /// Provides extension methods for <see cref="Observable{T}"/> to work with UI components.
    /// </summary>
    public partial class ObservableExtensions
    {
        /// <summary>
        /// Converts the value change events of a <see cref="TMP_InputField"/> into an observable sequence.
        /// Emits the current value of the input field and subsequent value changes as they occur.
        /// </summary>
        /// <param name="inputField">
        /// The <see cref="TMP_InputField"/> whose value change events are monitored and transformed into an observable sequence.
        /// </param>
        /// <returns>
        /// An observable sequence that emits the current and updated string values of the input field as they change.
        /// </returns>
        public static Observable<string> OnValueChangedAsObservable(this TMP_InputField inputField)
        {
            return Observable.Create<string, TMP_InputField>(inputField, static (observer, inputField) =>
            {
                observer.OnNext(inputField.text);
                return inputField.onValueChanged.AsObservable(inputField.destroyCancellationToken)
                    .Subscribe(observer);
            });
        }

        /// <summary>
        /// Binds a <see cref="Toggle"/> to a <see cref="ReactiveProperty{T}"/> of type <see cref="bool"/>,
        /// synchronizing the toggle's state with the property value.
        /// Sets the initial toggle state from the property and updates the property whenever the toggle changes.
        /// </summary>
        /// <param name="toggle">The <see cref="Toggle"/> to bind.</param>
        /// <param name="property">The <see cref="ReactiveProperty{T}"/> that receives toggle value changes.</param>
        /// <returns>
        /// An <see cref="IDisposable"/> that, when disposed, stops synchronization between the toggle and the property.
        /// </returns>
        public static IDisposable BindOnValueChanged(this Toggle toggle, ReactiveProperty<bool> property)
        {
            toggle.isOn = property.Value;
            return toggle.OnValueChangedAsObservable()
                .Subscribe(property, static (isOn, property) => property.Value = isOn);
        }
    }
}