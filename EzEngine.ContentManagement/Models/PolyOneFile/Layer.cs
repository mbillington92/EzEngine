namespace EzEngine.ContentManagement.Models.PolyOneFile;

public class Layer
{
    public int Order { get; set; }
    public string Name { get; set; } = null!;
    public string TypeName { get;set;} = null!;
    public int Type { get; set; }
    public string TextureName { get; set; } = null!;
    public int Texture { get; set; }
    public int VertexCount { get; set; }
    public List<float> VertsX { get; set; } = null!;
    public List<float> VertsY { get; set; } = null!;
    public List<float> VertsXTex { get; set; } = null!;
    public List<float> VertsYTex { get; set; } = null!;
    public List<string> VertsColour { get; set; } = null!;
    public List<float> VertsA { get; set; } = null!;
    public List<CustomVertexProperty> CustomVertexProperties { get; set; } = null!;
    public List<CustomLayerProperty> CustomLayerProperties { get; set; } = null!;
}