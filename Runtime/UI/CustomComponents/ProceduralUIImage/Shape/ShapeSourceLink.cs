using System.Collections.Generic;

namespace CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Shape
{
    internal sealed class ShapeSourceLink
    {
        internal ProceduralImage Source { get; private set; }

        private readonly List<ProceduralImage> _dependents = new();

        internal void Follow(ProceduralImage owner, ProceduralImage source)
        {
            if (Source == source)
                return;

            if (Source)
                Source.ShapeLink._dependents.Remove(owner);

            Source = source;

            if (source)
                source.ShapeLink._dependents.Add(owner);
        }

        internal void DirtyDependents()
        {
            foreach (var dependent in _dependents)
                dependent.SetVerticesDirty();
        }
    }
}
