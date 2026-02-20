using R3;
using TMPro;

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
    }
}