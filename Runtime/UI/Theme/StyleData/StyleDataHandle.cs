namespace CustomUtils.Runtime.UI.Theme.StyleData
{
    internal sealed class StyleDataHandle
    {
        internal int Row { get; private set; } = StyleDataTexture.NoRow;

        private StyleDataKey _key;

        internal void Apply(StyleDataKey key, bool isUsed)
        {
            if (!isUsed)
            {
                Clear();
                return;
            }

            if (Row != StyleDataTexture.NoRow && _key.Equals(key))
                return;

            var previousRow = Row;
            _key = key;
            Row = StyleDataTexture.Acquire(key);
            StyleDataTexture.Release(previousRow);
        }

        internal void Clear()
        {
            StyleDataTexture.Release(Row);
            Row = StyleDataTexture.NoRow;
        }
    }
}
