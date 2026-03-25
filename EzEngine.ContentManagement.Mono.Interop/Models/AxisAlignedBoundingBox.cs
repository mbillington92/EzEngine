using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EzEngine.ContentManagement.Mono.Interop.Models.Renderables;
using EzEngine.ContentManagement.Mono.Interop.Interfaces;

namespace EzEngine.ContentManagement.Mono.Interop.Models;

public class AxisAlignedBoundingBox : IVisualizableAsLineList
{
    public Vector3 MinimumExtents { get; private set; }
    public Vector3 MaximumExtents { get; private set; }

    public AxisAlignedBoundingBox(Vector3 min, Vector3 max)
    {
        MinimumExtents = new Vector3(min.X, min.Y, min.Z);
        MaximumExtents = new Vector3(max.X, max.Y, max.Z);
    }

    public AxisAlignedBoundingBox(Vector3[] vertices)
    {
        var minX = float.MaxValue;
        var minY = float.MaxValue;
        var minZ = float.MaxValue;
        var maxX = float.MinValue;
        var maxY = float.MinValue;
        var maxZ = float.MinValue;
        for (int i = 0; i < vertices.Length; i++)
        {
            minX = (int)Math.Min(vertices[i].X, minX);
            minY = (int)Math.Min(vertices[i].Y, minY);
            minZ = (int)Math.Min(vertices[i].Z, minZ);
            maxX = (int)Math.Max(vertices[i].X, maxX);
            maxY = (int)Math.Max(vertices[i].Y, maxY);
            maxZ = (int)Math.Max(vertices[i].Z, maxZ);
        }
        MinimumExtents = new Vector3(minX, minY, minZ);
        MaximumExtents = new Vector3(maxX, maxY, maxZ);
    }

    public bool PointIsWithinXYZ(float x, float y, float z)
    {
        if (PointIsWithinXY(x, y) &&
            z > MinimumExtents.Z && z < MaximumExtents.Z)
        {
            return true;
        }
        return false;
    }

    public bool PointIsWithinXY(float x, float y)
    {
        if (x > MinimumExtents.X && x < MaximumExtents.X &&
            y > MinimumExtents.Y && y < MaximumExtents.Y)
        {
            return true;
        }
        return false;
    }

    public bool PointIsWithinXYZ(Vector3 point)
    {
        if (point.X > MinimumExtents.X && point.X < MaximumExtents.X &&
            point.Y > MinimumExtents.Y && point.Y < MaximumExtents.Y &&
            point.Z > MinimumExtents.Z && point.Z < MaximumExtents.Z)
        {
            return true;
        }
        return false;
    }

    public bool PointIsWithinXY(Vector2 point)
    {
        if (point.X > MinimumExtents.X && point.X < MaximumExtents.X &&
            point.Y > MinimumExtents.Y && point.Y < MaximumExtents.Y)
        {
            return true;
        }
        return false;
    }

    public bool PointIsWithinXY(Point point)
    {
        if (point.X > MinimumExtents.X && point.X < MaximumExtents.X &&
            point.Y > MinimumExtents.Y && point.Y < MaximumExtents.Y)
        {
            return true;
        }
        return false;
    }

    public LineListPrimitive GetLineListVisualization(GraphicsDevice graphicsDevice, Color? overrideColour = null)
    {
        var vertices = new List<Vector3>();
        vertices.Add(new Vector3(MinimumExtents.X, MinimumExtents.Y, MinimumExtents.Z));
        vertices.Add(new Vector3(MaximumExtents.X, MinimumExtents.Y, MinimumExtents.Z));
        
        vertices.Add(new Vector3(MaximumExtents.X, MinimumExtents.Y, MinimumExtents.Z));
        vertices.Add(new Vector3(MaximumExtents.X, MaximumExtents.Y, MinimumExtents.Z));

        vertices.Add(new Vector3(MaximumExtents.X, MaximumExtents.Y, MinimumExtents.Z));
        vertices.Add(new Vector3(MinimumExtents.X, MaximumExtents.Y, MinimumExtents.Z));

        vertices.Add(new Vector3(MinimumExtents.X, MaximumExtents.Y, MinimumExtents.Z));
        vertices.Add(new Vector3(MinimumExtents.X, MinimumExtents.Y, MinimumExtents.Z));


        vertices.Add(new Vector3(MinimumExtents.X, MinimumExtents.Y, MaximumExtents.Z));
        vertices.Add(new Vector3(MaximumExtents.X, MinimumExtents.Y, MaximumExtents.Z));
        
        vertices.Add(new Vector3(MaximumExtents.X, MinimumExtents.Y, MaximumExtents.Z));
        vertices.Add(new Vector3(MaximumExtents.X, MaximumExtents.Y, MaximumExtents.Z));

        vertices.Add(new Vector3(MaximumExtents.X, MaximumExtents.Y, MaximumExtents.Z));
        vertices.Add(new Vector3(MinimumExtents.X, MaximumExtents.Y, MaximumExtents.Z));

        vertices.Add(new Vector3(MinimumExtents.X, MaximumExtents.Y, MaximumExtents.Z));
        vertices.Add(new Vector3(MinimumExtents.X, MinimumExtents.Y, MaximumExtents.Z));


        vertices.Add(new Vector3(MinimumExtents.X, MinimumExtents.Y, MinimumExtents.Z));
        vertices.Add(new Vector3(MinimumExtents.X, MinimumExtents.Y, MaximumExtents.Z));

        vertices.Add(new Vector3(MaximumExtents.X, MinimumExtents.Y, MinimumExtents.Z));
        vertices.Add(new Vector3(MaximumExtents.X, MinimumExtents.Y, MaximumExtents.Z));

        vertices.Add(new Vector3(MaximumExtents.X, MaximumExtents.Y, MinimumExtents.Z));
        vertices.Add(new Vector3(MaximumExtents.X, MaximumExtents.Y, MaximumExtents.Z));

        vertices.Add(new Vector3(MinimumExtents.X, MaximumExtents.Y, MinimumExtents.Z));
        vertices.Add(new Vector3(MinimumExtents.X, MaximumExtents.Y, MaximumExtents.Z));

        var colour = overrideColour ?? new Color(1.0F, 1.0F, 0.0F);
        var lineColours = vertices
            .Select(x => colour).ToArray();

        return new LineListPrimitive(graphicsDevice, [.. vertices], lineColours);
    }
}