using System.Runtime.CompilerServices;
using JetBrains.Annotations;

#if ZSTRING_INSTALLED
using Cysharp.Text;

#else
using System.Text;
#endif

namespace CustomUtils.Runtime.Formatter
{
    [PublicAPI]
    public ref struct StringBuilderScope
    {
#if ZSTRING_INSTALLED
        private Utf16ValueStringBuilder _zstringBuilder;
#else
        private StringBuilder _stringBuilder;
#endif

        public static StringBuilderScope Create(bool notNested = false)
        {
            var scope = new StringBuilderScope();
#if ZSTRING_INSTALLED
            scope._zstringBuilder = ZString.CreateStringBuilder(notNested);
#else
            scope._stringBuilder = new StringBuilder();
#endif
            return scope;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(char value)
        {
#if ZSTRING_INSTALLED
            _zstringBuilder.Append(value);
#else
            _stringBuilder.Append(value);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(string value)
        {
#if ZSTRING_INSTALLED
            _zstringBuilder.Append(value);
#else
            _stringBuilder.Append(value);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendLine(string value)
        {
#if ZSTRING_INSTALLED
            _zstringBuilder.AppendLine(value);
#else
            _stringBuilder.AppendLine(value);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(string value, int startIndex, int count)
        {
#if ZSTRING_INSTALLED
            _zstringBuilder.Append(value, startIndex, count);
#else
            _stringBuilder.Append(value, startIndex, count);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendLine()
        {
#if ZSTRING_INSTALLED
            _zstringBuilder.AppendLine();
#else
            _stringBuilder.AppendLine();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new string ToString()
        {
#if ZSTRING_INSTALLED
            return _zstringBuilder.ToString();
#else
            return _stringBuilder.ToString();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
#if ZSTRING_INSTALLED
            _zstringBuilder.Clear();
#else
            _stringBuilder.Clear();
#endif
        }

        public void Dispose()
        {
#if ZSTRING_INSTALLED
            _zstringBuilder.Dispose();
#endif
        }
    }
}