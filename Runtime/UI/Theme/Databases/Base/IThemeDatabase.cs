using System.Collections.Generic;

namespace CustomUtils.Runtime.UI.Theme.Databases.Base
{
    internal interface IThemeDatabase
    {
        List<string> GetColorNames();
        List<string> GetColorGuids();
    }
}
