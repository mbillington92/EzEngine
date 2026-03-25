namespace EzEngine.ContentManagement.Models.PolyOneFile;

public class LayerGroup
{
    public string Name { get; set; } = null!;
    public int GroupOrder { get; set; }
    public List<Layer> Layers { get; set; } = null!;
}