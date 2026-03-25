namespace EzEngine.ContentManagement.Models.PolyOneFile;

public class CustomProperties
{
    public List<string> InternalNames { get; set; } = null!;
    public List<string> FriendlyNames { get; set; } = null!;
    public List<int> Types { get; set; } = null!;
    public List<int> Levels { get; set; } = null!;
    public List<string> DefaultValues { get; set; } = null!;
}