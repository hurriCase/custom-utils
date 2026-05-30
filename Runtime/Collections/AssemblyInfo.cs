using System.Runtime.CompilerServices;
using ZLinq;

[assembly: InternalsVisibleTo("CustomUtils.Editor.Collections")]

[assembly: ZLinqDropIn("CustomUtils.Collections",
    DropInGenerateTypes.Array | DropInGenerateTypes.List | DropInGenerateTypes.Enumerable)]