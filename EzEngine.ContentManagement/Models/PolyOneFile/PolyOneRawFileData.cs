namespace EzEngine.ContentManagement.Models.PolyOneFile;

public class PolyOneRawFileData
{
    public string FileName { get; set; }
    public MetaData PolyOneMeta { get; set; } = null!;
    public List<LayerGroup> LayerGroups { get; set; } = null!;
    public CustomProperties CustomProperties { get; set; } = null!;
    public AdditionalFileData? AdditionalFileData { get; set; }
}