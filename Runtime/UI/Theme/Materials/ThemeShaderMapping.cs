using System;
using UnityEngine;

namespace CustomUtils.Runtime.UI.Theme.Materials
{
    [Serializable]
    internal struct ThemeShaderMapping
    {
        [field: SerializeField] internal Shader BaseShader { get; private set; }
        [field: SerializeField] internal Shader ThemeShader { get; private set; }
    }
}
