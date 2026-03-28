using Microsoft.Xna.Framework;

namespace EzEngine.ContentManagement.Mono.Interop.Models;

[Obsolete("Use ProcessedPolyOneFilePointLightSet")]
public class ProcessedPolyOneFilePointLight
{
    public Vector3 Position { get; set; }
    public Color Colour { get; set; }
    public double FalloffDistance { get; set; }
    public readonly ProcessedPolyOneFile Parent;

    public ProcessedPolyOneFilePointLight(float x, float y, float z, Color colour, double falloffDistance, ProcessedPolyOneFile parent)
    {
        Position = new Vector3(x, y, z);
        Colour = colour;
        FalloffDistance = falloffDistance;
        Parent = parent;
    }
}