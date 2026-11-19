using System.Runtime.CompilerServices;
using ZLinq;

[assembly: InternalsVisibleTo("CustomUtils.Editor.Windows")]

[assembly: ZLinqDropIn("CustomUtils.Runtime.UI.Windows",
    DropInGenerateTypes.Array | DropInGenerateTypes.List | DropInGenerateTypes.Enumerable)]
