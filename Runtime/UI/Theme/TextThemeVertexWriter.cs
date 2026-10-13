using System.Collections.Generic;
using CustomUtils.Runtime.Attributes;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.UI.Theme.Materials;
using TMPro;
using UnityEngine;

namespace CustomUtils.Runtime.UI.Theme
{
    // TextMesh Pro builds its own mesh and skips mesh modifiers, so text gets its theme vertex data from here.
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMP_Text), typeof(ThemeComponent))]
    internal sealed class TextThemeVertexWriter : MonoBehaviour
    {
        [SerializeField, Self] private TMP_Text _text;
        [SerializeField, Self] private ThemeComponent _themeComponent;

        private readonly ThemeVertexWriter _vertexWriter = new();
        private readonly List<Vector4> _themeData = new();
        private readonly List<Vector4> _gradientSettings = new();

        private void OnEnable()
        {
            _text = _text.AsNullable() ?? GetComponent<TMP_Text>();
            _themeComponent = _themeComponent.AsNullable() ?? GetComponent<ThemeComponent>();

            _text.OnPreRenderText += WriteVertices;
            _text.SetAllDirty();
        }

        private void WriteVertices(TMP_TextInfo textInfo)
        {
            var rect = _text.rectTransform.rect;
            var gradientVertexData = _themeComponent.GetGradientVertexData();
            var gradientSettings = _vertexWriter.GetGradientSettings(gradientVertexData);

            for (var i = 0; i < textInfo.materialCount; i++)
            {
                var meshInfo = textInfo.meshInfo[i];
                if (!meshInfo.mesh)
                    continue;

                _themeData.Clear();
                _gradientSettings.Clear();

                foreach (var position in meshInfo.vertices)
                {
                    var themeData = _vertexWriter.GetThemeData(position, rect, gradientVertexData);
                    _themeData.Add(themeData);
                    _gradientSettings.Add(gradientSettings);
                }

                // The extra channels must match the vertex count, which TMP only uploads after this callback.
                meshInfo.mesh.vertices = meshInfo.vertices;
                meshInfo.mesh.SetUVs(ThemeVertexWriter.ThemeDataChannel, _themeData);
                meshInfo.mesh.SetUVs(ThemeVertexWriter.GradientSettingsChannel, _gradientSettings);
            }

            _vertexWriter.EnableShaderChannels(_text);
        }

        private void OnDisable()
        {
            _text.OnPreRenderText -= WriteVertices;
            _text.SetAllDirty();
        }
    }
}
