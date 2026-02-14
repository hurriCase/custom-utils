using JetBrains.Annotations;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CustomUtils.Runtime.ResponseTypes
{
    /// <summary>
    /// Result of a validation or operation with a success / failure state.
    /// </summary>
    [PublicAPI]
    public readonly struct Result
    {
        /// <summary>
        /// True if the operation was successful.
        /// </summary>

        public bool IsValid { get; }

        /// <summary>
        /// Error message when the operation failed, or null when successful.
        /// </summary>

        public string Message { get; }

        /// <summary>
        /// Creates a valid result indicating successful operation.
        /// </summary>
        public static Result Valid(string message = null) => new(true, message);

        /// <summary>
        /// Creates an invalid result with the specified error message.
        /// </summary>
        public static Result Invalid(string message = null) => new(false, message);

        /// <summary>
        /// Implicitly converts to boolean (true if valid).
        /// </summary>
        public static implicit operator bool(Result result) => result.IsValid;

        /// <summary>
        /// Implicitly converts to error message string.
        /// </summary>
        public static implicit operator string(Result result) => result.Message;

        public Result(bool isValid, string message)
        {
            IsValid = isValid;
            Message = message;
        }

#if UNITY_EDITOR
        internal void DisplayMessage()
        {
            if (string.IsNullOrEmpty(Message))
                return;

            var title = IsValid ? "Success" : "Error";
            EditorUtility.DisplayDialog(title, Message, "OK");
        }
#endif
    }

    /// <summary>
    /// Result of a validation or operation with a success/failure state and optional data.
    /// </summary>
    [PublicAPI]
    public readonly struct Result<T>
    {
        public bool IsValid { get; }

        public string Message { get; }

        public T Data { get; }

        public static Result<T> Valid(T data, string message = null) => new(true, data, message);

        public static Result<T> Invalid(string message = null) => new(false, default, message);

        public static implicit operator bool(Result<T> result) => result.IsValid;

        public static implicit operator string(Result<T> result) => result.Message;

        public Result(T data)
        {
            IsValid = true;
            Data = data;
            Message = null;
        }

        public Result(T data, string message)
        {
            IsValid = true;
            Data = data;
            Message = message;
        }

        public Result(bool isValid, T data, string message)
        {
            IsValid = isValid;
            Data = data;
            Message = message;
        }

#if UNITY_EDITOR
        internal void DisplayMessage()
        {
            if (string.IsNullOrEmpty(Message))
                return;

            var title = IsValid ? "Success" : "Error";
            EditorUtility.DisplayDialog(title, Message, "OK");
        }
#endif
    }
}