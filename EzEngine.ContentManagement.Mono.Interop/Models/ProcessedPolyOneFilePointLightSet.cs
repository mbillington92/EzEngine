using EzEngine.ContentManagement.Models.PolyOneFile;
using EzEngine.ContentManagement.Mono.Interop.Interfaces;
using EzEngine.ContentManagement.Mono.Interop.Models.Renderables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EzEngine.ContentManagement.Mono.Interop.Models;

public class ProcessedPolyOneFilePointLightSet : IVisualizableAsLineList
{
    public Vector3[] Positions { get; private set; }
    public Color[] Colours { get; private set; }
    public float[] FalloffDistance { get; private set; }
    public int PointLightCount { get; private set; }
    public readonly ProcessedPolyOneFile Parent;
    private bool OffsetIsRemoved = false;

    public ProcessedPolyOneFilePointLightSet(Layer rawLayer, ProcessedPolyOneFile parent)
    {
        Parent = parent;

        var falloffPropertyIndex = rawLayer.CustomVertexProperties
            .FindIndex(prop => prop.Name.Equals("PointLightFalloffDistance", StringComparison.CurrentCultureIgnoreCase));
        var pointLightsFalloff = falloffPropertyIndex >= 0
            ? rawLayer.CustomVertexProperties[falloffPropertyIndex].Values
                .Select(float.Parse)
                .ToArray()
            : throw new InvalidOperationException("Point Lights layer didn't specify a falloff distance custom vertex property");

        var zPropertyIndex = rawLayer.CustomVertexProperties
            .FindIndex(prop => prop.Name.Equals("z", StringComparison.CurrentCultureIgnoreCase));
        var vertsZ = zPropertyIndex >= 0
            ? rawLayer.CustomVertexProperties[zPropertyIndex].Values
                .Select(float.Parse)
                .ToArray()
            : rawLayer.VertsX
                .Select(x => 0.0F)
                .ToArray();

        var vertsXFlipped = rawLayer.VertsX
            .Select(x => -x)
            .ToArray();

        PointLightCount = rawLayer.VertexCount / 3;
        Positions = new Vector3[PointLightCount];
        Colours = new Color[PointLightCount];
        FalloffDistance = new float[PointLightCount];
        var currentIndex = 0;
        for (var i = 0; i < rawLayer.VertexCount; i += 3)
        {
            Positions[currentIndex] = new Vector3(vertsXFlipped[i], rawLayer.VertsY[i], vertsZ[i]);
            Colours[currentIndex] = Converters.ConvertFromHexLegacy(rawLayer.VertsColour[i]);
            FalloffDistance[currentIndex] = pointLightsFalloff[i];
            currentIndex++;
        }
    }

    public void RemoveOffset(Vector2 min, Vector2 max)
    {
        if (OffsetIsRemoved) return;

        Positions = Positions.Select(p => new Vector3(
            p.X - max.X, p.Y - min.Y, p.Z
        )).ToArray();

        OffsetIsRemoved = true;
    }

    public LineListPrimitive GetLineListVisualization(GraphicsDevice graphicsDevice, Color? colourOverride = null)
    {
        var pointLightsSymbolicVertices = new List<Vector3>();
        var pointLightsColour = new List<Color>();
        for (var i = 0; i < PointLightCount; i++)
        {
            var lightPos = Positions[i];
            pointLightsSymbolicVertices.Add(new Vector3(lightPos.X - 16.0F, lightPos.Y, lightPos.Z));
            pointLightsSymbolicVertices.Add(new Vector3(lightPos.X + 16.0F, lightPos.Y, lightPos.Z));
            pointLightsSymbolicVertices.Add(new Vector3(lightPos.X, lightPos.Y - 16.0F, lightPos.Z));
            pointLightsSymbolicVertices.Add(new Vector3(lightPos.X, lightPos.Y + 16.0F, lightPos.Z));
            pointLightsSymbolicVertices.Add(new Vector3(lightPos.X, lightPos.Y, lightPos.Z - 16.0F));
            pointLightsSymbolicVertices.Add(new Vector3(lightPos.X, lightPos.Y, lightPos.Z + 16.0F));

            for (var j = 0; j < 6; j++)
            {
                pointLightsColour.Add(Colours[i]);
            }
        }

        return new LineListPrimitive(graphicsDevice, [.. pointLightsSymbolicVertices], [.. pointLightsColour]);
    }
}