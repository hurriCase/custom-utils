using System.Runtime.CompilerServices;
using ZLinq;

[assembly: InternalsVisibleTo("CustomUtils.Editor")]
[assembly: InternalsVisibleTo("CustomUtils.Windows")]

[assembly: ZLinqDropIn("CustomUtils.Runtime",
    DropInGenerateTypes.Array | DropInGenerateTypes.List | DropInGenerateTypes.Enumerable)]