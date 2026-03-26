using EzEngine.ContentManagement.Mono.Interop.Models;
using Microsoft.Xna.Framework;

namespace EzEngine.ContentManagement.Mono.Interop;

public static class Helpers
{
    //https://stackoverflow.com/questions/12891516/math-calculation-to-retrieve-angle-between-two-points
    public static double PointDirection(double x1, double y1, double x2, double y2)
    {
        var xDiff = x2 - x1;
        var yDiff = y2 - y1;
        return Math.Atan2(yDiff, xDiff) + Math.PI; // * 180.0D / Math.PI;
    }

    public static double DistanceSquared(Vector2 p)
    {
        return p.X * p.X + p.Y * p.Y;
    }

    /// <summary>
    /// Returns the squared distance between 0,0,0 and p. (Optimization of true distance)
    /// Remember to square the other side of any comparisons on the result.
    /// Usually not directly useful when used in calcuations as part of assigning values.
    /// When the result is needed in such scenarios more than once, storing in a separate variable with <see cref="Math.Sqrt"> is highly recommended.
    /// </summary>
    public static double DistanceSquared(Vector3 p)
    {
        return p.X * p.X + p.Y * p.Y + p.Z * p.Z;
    }

    /// <summary>
    /// Returns the squared distance between p1 and p2. (Optimization of true distance)
    /// Remember to square the other side of any comparisons on the result.
    /// </summary>
    public static double DistanceSquared(Vector3 p1, Vector3 p2)
    {
        return DistanceSquared(p1.X, p1.Y, p1.Z, p2.X, p2.Y, p2.Z);
    }

    public static double DistanceSquared(double pX, double pY, double pZ)
    {
        return pX * pX + pY * pY + pZ * pZ;
    }

    public static double DistanceSquared(double x1, double y1, double z1, double x2, double y2, double z2)
    {
        var xDistance = x2 - x1;
        var yDistance = y2 - y1;
        var zDistance = z2 - z1;
        return xDistance * xDistance + yDistance * yDistance + zDistance * zDistance;
    }

    public static double DistanceSquared(double x1, double y1, double x2, double y2)
    {
        var xDistance = x2 - x1;
        var yDistance = y2 - y1;
        return xDistance * xDistance + yDistance * yDistance;
    }

    public static Vector3[] CalculateSurfaceNormals(Vector3[] vertexPositions)
    {
        var normals = new List<Vector3>();
        for (var i = 0; i < vertexPositions.Length; i += 3)
        {
            var v0v1x = vertexPositions[i + 1].X - vertexPositions[i].X;
            var v0v1y = vertexPositions[i + 1].Y - vertexPositions[i].Y;
            var v0v1z = vertexPositions[i + 1].Z - vertexPositions[i].Z;

            var v0v2x = vertexPositions[i + 2].X - vertexPositions[i].X;
            var v0v2y = vertexPositions[i + 2].Y - vertexPositions[i].Y;
            var v0v2z = vertexPositions[i + 2].Z - vertexPositions[i].Z;

            var nX = v0v1y * v0v2z - v0v1z * v0v2y;
            var nY = v0v1z * v0v2x - v0v1x * v0v2z;
            var nZ = v0v1x * v0v2y - v0v1y * v0v2x;

            var crossProductLength = (float)Math.Sqrt(Helpers.DistanceSquared(nX, nY, nZ));

            nX /= crossProductLength;
            nY /= crossProductLength;
            nZ /= crossProductLength;

            normals.Add(new Vector3(-nX, -nY, -nZ));
            normals.Add(new Vector3(-nX, -nY, -nZ));
            normals.Add(new Vector3(-nX, -nY, -nZ));
        }
        return normals.ToArray();
    }

    public static AxisAlignedBoundingBox GenerateAxisAlignedBoundingBox(Vector3[] vertices)
    {
        var minX = float.MaxValue;
        var minY = float.MaxValue;
        var minZ = float.MaxValue;
        var maxX = float.MinValue;
        var maxY = float.MinValue;
        var maxZ = float.MinValue;
        for (int i = 0; i < vertices.Length; i++)
        {
            minX = Math.Min(vertices[i].X, minX);
            minY = Math.Min(vertices[i].Y, minY);
            minZ = Math.Min(vertices[i].Z, minZ);
            maxX = Math.Max(vertices[i].X, maxX);
            maxY = Math.Max(vertices[i].Y, maxY);
            maxZ = Math.Max(vertices[i].Z, maxZ);
        }
        return new AxisAlignedBoundingBox(
            new Vector3(minX, minY, minZ),
            new Vector3(maxX, maxY, maxZ)
        );
    }
}