using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EzEngine.ContentManagement.Models.PolyOneFile;
using EzEngine.ContentManagement.Mono.Interop.Models.Renderables;
using EzEngine.ContentManagement.Mono.Interop.Interfaces;

namespace EzEngine.ContentManagement.Mono.Interop.Models;

/// <summary>
/// A set of vertically-oriented triangular prisms which represents collidable spaces in a PolyOne level or model file.
/// The triangles formed at the top and bottom sides can be sloped.
/// </summary>
public class ProcessedPolyOneFileVolumeSet : IVisualizableAsLineList
{
    /// <summary>
    /// Maximum extents of each vertically-oriented triangular prism.
    /// In other words, when combined with Max, an imaginary cube between both would fully enclose the given volume.
    /// </summary>
    public Vector3[] Min { get; private set; }
    /// <summary>
    /// Maximum extents of each vertically-oriented triangular prism
    /// </summary>
    public Vector3[] Max { get; private set; }
    /// <summary>
    /// Minimum extents of all vertically-oriented triangular prisms in the volume set
    /// In other words, when combined with Max, an imaginary cube between both would fully enclose all volumes in the given set.
    /// </summary>
    public Vector3 OverallMin { get; private set; }
    /// <summary>
    /// Maximum extents of all vertically-oriented triangular prisms in the volume set
    /// </summary>
    public Vector3 OverallMax { get; private set; }
    /// <summary>
    /// The vertices of the lower triangle of each vertically-oriented triangular prism
    /// </summary>
    public Vector3[] LowerVertices { get; private set; }
    /// <summary>
    /// The vertices of the upper triangle of each vertically-oriented triangular prism
    /// </summary>
    public Vector3[] UpperVertices { get; private set; }
    public int VertexCount { get; private set; }
    public readonly ProcessedPolyOneFile Parent;
    public AxisAlignedBoundingBox[] AxisAlignedBoundingBoxes { get; private set; }
    /// <summary>
    /// Bounding box for the whole set, i.e. encompassing all volumes in this set
    /// </summary>
    public AxisAlignedBoundingBox OverallAxisAlignedBoundingBox { get; private set; }

    private bool OffsetIsRemoved = false;

    static bool CompareLess(double xy1, double xy2)
    {
        return xy1 < xy2;
    }

    static bool CompareGreater(double xy1, double xy2)
    {
        return xy1 > xy2;
    }

    private Func<double, double, bool>[] _compareIntersectX;
    private Func<double, double, bool>[] _compareIntersectY;

    public ProcessedPolyOneFileVolumeSet(Layer layer, ProcessedPolyOneFile parent)
    {
        var lowerZValues = layer.CustomVertexProperties.Single(x => x.Name == "Z")
            .Values.Select(float.Parse!).ToArray();
        var upperZValues = layer.CustomVertexProperties.Single(x => x.Name == "ZTop")
            .Values.Select(float.Parse!).ToArray();

        LowerVertices = lowerZValues.Select((x, index) => new Vector3(
            -layer.VertsX[index],
            layer.VertsY[index],
            lowerZValues[index]
        )).ToArray();
        UpperVertices = upperZValues.Select((x, index) => new Vector3(
            -layer.VertsX[index],
            layer.VertsY[index],
            upperZValues[index]
        )).ToArray();


        VertexCount = lowerZValues.Length;
        Parent = parent;

        RebuildAxisAlignedBoundingBoxes();
        DetermineCollisonPlanes();
    }

    public void RemoveOffset(Vector2 min, Vector2 max)
    {
        if (OffsetIsRemoved) return;
        
        LowerVertices = LowerVertices.Select(v => new Vector3(
            v.X - max.X, v.Y - min.Y, v.Z
        )).ToArray();
        UpperVertices = UpperVertices.Select(v => new Vector3(
            v.X - max.X, v.Y - min.Y, v.Z
        )).ToArray();

        RebuildAxisAlignedBoundingBoxes();

        OffsetIsRemoved = true;
    }

    public void ApplyTransformation(Vector3 offset, double sine, double cosine, Vector3 nonUniformScale, Vector3 skewNormal)
    {
        for (int i = 0; i < LowerVertices.Length; i++)
        {
            var initialPosition = new Vector3(LowerVertices[i].X, LowerVertices[i].Y, LowerVertices[i].Z);
            LowerVertices[i].X = (float)(offset.X + initialPosition.X * nonUniformScale.X * cosine + initialPosition.Y * nonUniformScale.Y * -sine);
            LowerVertices[i].Y = (float)(offset.Y + initialPosition.Y * nonUniformScale.Y * cosine + initialPosition.X * nonUniformScale.X * sine);
            UpperVertices[i].X = LowerVertices[i].X;
            UpperVertices[i].Y = LowerVertices[i].Y;

            var v0x = offset.X;
            var v0y = offset.Y;
            var v0z = offset.Z;
            var nX = skewNormal.X;
            var nY = skewNormal.Y;
            var nZ = skewNormal.Z;
            var skewTargetZ = v0z - ((LowerVertices[i].X - v0x) * nX + (LowerVertices[i].Y - v0y) * nY) / nZ;
            LowerVertices[i].Z = initialPosition.Z + skewTargetZ;
            var skewTargetUpperZ = v0z - ((UpperVertices[i].X - v0x) * nX + (UpperVertices[i].Y - v0y) * nY) / nZ;
            UpperVertices[i].Z = UpperVertices[i].Z + skewTargetUpperZ;
        }
        RebuildAxisAlignedBoundingBoxes();
        DetermineCollisonPlanes();
    }

    public void RebuildAxisAlignedBoundingBoxes()
    {
        var currentAABB = 0;
        AxisAlignedBoundingBoxes = new AxisAlignedBoundingBox[VertexCount / 3];
        for (int i = 0; i < VertexCount; i += 3)
        {
            AxisAlignedBoundingBoxes[currentAABB] = new AxisAlignedBoundingBox(
                [
                    LowerVertices[i],
                    LowerVertices[i + 1],
                    LowerVertices[i + 2],
                    UpperVertices[i],
                    UpperVertices[i + 1],
                    UpperVertices[i + 2]
                ]
            );
            currentAABB++;
        }
        var allVertices = LowerVertices.Select(v => new Vector3(v.X, v.Y, v.Z)).ToList();
        allVertices.AddRange(UpperVertices);
        OverallAxisAlignedBoundingBox = new AxisAlignedBoundingBox([.. allVertices]);
    }

    public int? PointIsWithinAnyVolume(Vector3 point)
    {
        if (OverallAxisAlignedBoundingBox.PointIsWithinXYZ(point))
        {
            for (var i = 0; i < AxisAlignedBoundingBoxes.Length; i++)
            {
                if (AxisAlignedBoundingBoxes[i]
                    .PointIsWithinXYZ(point))
                {
                    var rootVertexIndex = i * 3;
                    if (PointCrossesPlane(point.X, point.Y, 
                        rootVertexIndex, rootVertexIndex + 1) &&
                        PointCrossesPlane(point.X, point.Y,
                        rootVertexIndex + 1, rootVertexIndex + 2) &&
                        PointCrossesPlane(point.X, point.Y,
                        rootVertexIndex + 2, rootVertexIndex))
                    {
                        return i;
                    }
                }
            }
        }
        return null;
    }

    /// <summary>
    /// When a collision with a volume has occurred, the previous position of whatever collided should always
    /// collide with 2 out of 3 of the edges of the triangle that the volume is formed of. Therefore, it can 
    /// be used for collision resolution to deduce which edge the collision took place on and respond 
    /// appropriately.
    /// </summary>
    /// <param name="previousPosition">The previous or starting position of the point which is known to have 
    /// collided</param>
    /// <param name="volumeIndex">The index of the volume which the collision occurred with
    /// </param>
    /// <returns>2D vector of the edge that previously was NOT collided with</returns>
    public Vector2? GetLastNonCollidedSide(Vector3 previousPosition, int volumeIndex)
    {
        var rootVertexIndex = volumeIndex * 3;
        if (!PointCrossesPlane(previousPosition.X, previousPosition.Y, 
            rootVertexIndex, rootVertexIndex + 1))
        {
            //TODO: Make this support volumes with skewed walls in the future if possible
            return new Vector2(
                LowerVertices[rootVertexIndex + 1].X - LowerVertices[rootVertexIndex].X,
                LowerVertices[rootVertexIndex + 1].Y - LowerVertices[rootVertexIndex].Y);
        }
        if (!PointCrossesPlane(previousPosition.X, previousPosition.Y,
            rootVertexIndex + 1, rootVertexIndex + 2))
        {
            return new Vector2(
                LowerVertices[rootVertexIndex + 2].X - LowerVertices[rootVertexIndex + 1].X,
                LowerVertices[rootVertexIndex + 2].Y - LowerVertices[rootVertexIndex + 1].Y);
        }
        if (!PointCrossesPlane(previousPosition.X, previousPosition.Y,
            rootVertexIndex + 2, rootVertexIndex))
        {
            return new Vector2(
                LowerVertices[rootVertexIndex].X - LowerVertices[rootVertexIndex + 2].X,
                LowerVertices[rootVertexIndex].Y - LowerVertices[rootVertexIndex + 2].Y);
        }
        return null;
    }

    /// <summary>
    /// Tests whether a point crosses a plane between two other points
    /// </summary>
    /// <param name="pX"></param>
    /// <param name="pY"></param>
    /// <param name="v0Index"></param>
    /// <param name="v1Index"></param>
    /// <returns></returns>
    public bool PointCrossesPlane(float pX, float pY, int v0Index, int v1Index)
    {
        var x1 = UpperVertices[v0Index].X;
        var y1 = UpperVertices[v0Index].Y;
        var x2 = UpperVertices[v1Index].X;
        var y2 = UpperVertices[v1Index].Y;

        var currentToNextX = x2 - x1;
        var currentToNextY = y2 - y1;

        var currentToPointX = pX - x1;
        var currentToPointY = pY - y1;

        var pointFactorX = currentToPointX / currentToNextX;
        var pointFactorY = currentToPointY / currentToNextY;

        var axisIntersectX = x1 + (currentToNextX * pointFactorY);
        var axisIntersectY = y1 + (currentToNextY * pointFactorX);

        var result = _compareIntersectX[v0Index](pX, axisIntersectX) && 
            _compareIntersectY[v0Index](pY, axisIntersectY);

        return result;
    }

    /// <summary>
    /// Collision detection with volumes works by checking which side of a volume triangle's edge a point
    /// is on. If the point is on the same side of the triangle which has the vertex that isn't included
    /// in the test, then it's potentially inside of the triangle. We don't know if I point is definitively
    /// inside of the triangle until all three edges are tested in this way.
    /// The comparison to determine which side of the triangle the point is on is always the same,
    /// assuming no change in rotation or shape of the triangle, so the way to compare is stored per edge
    /// for the X and Y axes.
    /// Detection on the Z axis is done differently, but this will be introduced later.
    /// </summary>
    private void DetermineCollisonPlanes()
    {
        _compareIntersectX = new Func<double, double, bool>[LowerVertices.Length];
        _compareIntersectY = new Func<double, double, bool>[LowerVertices.Length];
        for (var i = 0; i < LowerVertices.Length; i += 3)
        {
            DetermineCollisonPlane(i, i + 1, i + 2);
            DetermineCollisonPlane(i + 1, i + 2, i);
            DetermineCollisonPlane(i + 2, i, i + 1);
        }
    }

    private void DetermineCollisonPlane(int vCurrent, int vNext, int vOther)
    {
        var currentToNextX = LowerVertices[vNext].X - LowerVertices[vCurrent].X;
        var currentToNextY = LowerVertices[vNext].Y - LowerVertices[vCurrent].Y;
        var pointFactorX = (LowerVertices[vOther].X - LowerVertices[vCurrent].X) / currentToNextX;
        var pointFactorY = (LowerVertices[vOther].Y - LowerVertices[vCurrent].Y) / currentToNextY;
        var axisIntersectX = LowerVertices[vCurrent].X + (currentToNextX * pointFactorY);
        var axisIntersectY = LowerVertices[vCurrent].Y + (currentToNextY * pointFactorX);

        _compareIntersectX[vCurrent] = LowerVertices[vOther].X < axisIntersectX
            ? CompareLess
            : CompareGreater;

        _compareIntersectY[vCurrent] = LowerVertices[vOther].Y < axisIntersectY
            ? CompareLess
            : CompareGreater;
    }

    public LineListPrimitive GetLineListVisualization(GraphicsDevice graphicsDevice, Color? overrideColour = null)
    {
        var volumeLineVertices = new List<Vector3>();
        for (var i = 0; i < VertexCount; i += 3)
        {
            volumeLineVertices.Add(UpperVertices[i]);
            volumeLineVertices.Add(UpperVertices[i + 1]);
            volumeLineVertices.Add(UpperVertices[i + 1]);
            volumeLineVertices.Add(UpperVertices[i + 2]);
            volumeLineVertices.Add(UpperVertices[i + 2]);
            volumeLineVertices.Add(UpperVertices[i]);

            volumeLineVertices.Add(LowerVertices[i]);
            volumeLineVertices.Add(LowerVertices[i + 1]);
            volumeLineVertices.Add(LowerVertices[i + 1]);
            volumeLineVertices.Add(LowerVertices[i + 2]);
            volumeLineVertices.Add(LowerVertices[i + 2]);
            volumeLineVertices.Add(LowerVertices[i]);

            volumeLineVertices.Add(UpperVertices[i]);
            volumeLineVertices.Add(LowerVertices[i]);

            volumeLineVertices.Add(UpperVertices[i + 1]);
            volumeLineVertices.Add(LowerVertices[i + 1]);

            volumeLineVertices.Add(UpperVertices[i + 2]);
            volumeLineVertices.Add(LowerVertices[i + 2]);
        }
        var colour = overrideColour ?? new Color(1.0F, 0.0F, 0.0F);
        var normalLineColours = volumeLineVertices
            .Select(x => colour).ToArray();

        return new LineListPrimitive(graphicsDevice, [.. volumeLineVertices], normalLineColours);
    }
}