namespace EzEngine.ContentManagement.Models.PolyOneFile;

public class PolyOneFileData
{
    public MetaData PolyOneMeta { get; set; } = null!;
    public List<LayerGroup> LayerGroups { get; set; } = null!;
    public CustomProperties CustomProperties { get; set; } = null!;
}