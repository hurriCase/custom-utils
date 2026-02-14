using System;
using UnityEngine;

namespace CustomUtils.Runtime.Downloader
{
    [Serializable]
    public class Sheet
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public long Id { get; private set; }
        [field: SerializeField] public TextAsset TextAsset { get; private set; }
        [field: SerializeField] public long ContentLength { get; private set; }

        internal void Initialize(string name, long id)
        {
            Name = name;
            Id = id;
        }

        public void UpdateSheetData(TextAsset textAsset, long contentLength)
        {
            TextAsset = textAsset;
            ContentLength = contentLength;
        }

        public bool HasChanged(long newContentLength) => ContentLength != newContentLength;
    }
}