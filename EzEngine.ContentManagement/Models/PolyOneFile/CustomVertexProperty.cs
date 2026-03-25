namespace EzEngine.ContentManagement.Models.PolyOneFile;

public class CustomVertexProperty
{
    public string Name { get; set; } = null!;
    public List<string?> Values { get; set; } = null!;
    public int RowIndex { get; set; }
    public int GlobalIndex { get; set; }
}