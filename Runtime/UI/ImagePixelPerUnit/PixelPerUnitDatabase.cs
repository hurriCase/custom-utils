using System.Collections.Generic;
using CustomUtils.Runtime.AssetLoader;
using CustomUtils.Runtime.CustomTypes.Singletons;
using CustomUtils.Runtime.Other;
using UnityEngine;
using ZLinq;

namespace CustomUtils.Runtime.UI.ImagePixelPerUnit
{
    [Resource(
        ResourcePaths.ImagePixelPerUnitFullPath,
        ResourcePaths.ImagePixelPerUnitDatabaseAssetName,
        ResourcePaths.ImagePixelPerUnitResourcePath
    )]
    internal sealed class PixelPerUnitDatabase : SingletonScriptableObject<PixelPerUnitDatabase>
    {
        [field: SerializeField] internal List<PixelPerUnitData> PixelPerUnitData { get; private set; }

        internal List<string> GetPixelPerUnitTypeNames() => PixelPerUnitData?.Select(static data => data.Name).ToList();

        internal PixelPerUnitData GetPixelPerUnitData(string pixelPerUnityTypeName)
            => PixelPerUnitData.FirstOrDefault(data => data.Name == pixelPerUnityTypeName);
    }
}