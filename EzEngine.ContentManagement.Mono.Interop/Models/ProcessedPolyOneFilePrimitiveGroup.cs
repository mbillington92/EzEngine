using Microsoft.Xna.Framework;

namespace EzEngine.ContentManagement.Mono.Interop.Models;

public class ProcessedPolyOneFilePrimitiveGroup
{
    public string Name { get; private set; } = null!;
    public int GroupOrder { get; private set; }
    public ProcessedPolyOneFilePrimitive[]? Primitives { get; private set; }
    public readonly ProcessedPolyOneFile Parent;

    public ProcessedPolyOneFilePrimitiveGroup(ContentManagement.Models.PolyOneFile.LayerGroup layerGroup, ProcessedPolyOneFile parent)
    {
        Parent = parent;
        Name = layerGroup.Name;
        GroupOrder = layerGroup.GroupOrder;
        Primitives = layerGroup.Layers?
            .Where(x => x.VertexCount > 0 && parent.NonRenderablePrimitiveFilter.Contains(x.Name) == false)
            .Select(x => new ProcessedPolyOneFilePrimitive(x, this))
            .ToArray();
    }

    public void ApplyTransformation(Vector3 offset, double sine, double cosine, Vector3 nonUniformScale, Vector3 skewNormal)
    {
        foreach (var primitive in Primitives)
        {
            primitive.ApplyTransformation(offset, sine, cosine, nonUniformScale, skewNormal);
        }
    }
}