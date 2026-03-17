namespace CustomUtils.Runtime.UI.Theme.Databases.Base
{
    internal interface IThemeDatabase<TColor>
    {
        bool TryGetColorByGuid(string guid, out TColor color);
    }
}