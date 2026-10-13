namespace CustomUtils.Runtime.UI.Theme.Databases.Base
{
    internal interface IThemeDatabase<TColor> : IThemeDatabase
    {
        bool TryGetColorByGuid(string guid, out TColor color);
    }
}
