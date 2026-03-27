using EzEngine.ContentManagement.Mono.Interop.Models.Renderables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EzEngine.ContentManagement.Mono.Interop.Extensions;

public static class Vector3Extensions
{
    public static LineListPrimitive GetLineListVisualization(this Vector3 offset, GraphicsDevice graphicsDevice, Vector3 vector, Color? overrideColour = null)
    {
        var vectorLineVertices = new List<Vector3>();
        vectorLineVertices.Add(offset);
        vectorLineVertices.Add(vector);

        var colour = overrideColour ?? new Color(1.0F, 1.0F, 0.0F);
        var lineColours = vectorLineVertices
            .Select(x => colour).ToArray();

        return new LineListPrimitive(graphicsDevice, [.. vectorLineVertices], lineColours);
    }

    public static LineListPrimitive GetLineListVisualization(this Vector3[] basePositions, GraphicsDevice graphicsDevice, Vector3 vector, Color? overrideColour = null)
    {
        var vectorLineVertices = new Vector3[basePositions.Length * 2];
        for (var i = 0; i < basePositions.Length; i++)
        {
            vectorLineVertices[i * 2] = new Vector3(basePositions[i].X, basePositions[i].Y, basePositions[i].Z);
            vectorLineVertices[i * 2 + 1] = new Vector3(
                basePositions[i].X + vector.X,
                basePositions[i].Y + vector.Y,
                basePositions[i].Z + vector.Z);
        }
        var colour = overrideColour ?? new Color(1.0F, 1.0F, 0.0F);
        var lineColours = vectorLineVertices
            .Select(x => colour).ToArray();

        return new LineListPrimitive(graphicsDevice, [.. vectorLineVertices], lineColours);
    }
}