using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EzEngine.ContentManagement.Models.PolyOneFile;
using EzEngine.ContentManagement.Mono.Interop.Models.Renderables;

namespace EzEngine.ContentManagement.Mono.Interop.Models;

/// <summary>
/// A set of vertically-oriented triangular prisms which represents collidable spaces in a PolyOne level or model file.
/// The triangles formed at the top and bottom sides can be sloped.
/// </summary>
public class ProcessedPolyOneFileVolumeSet
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
            //var currentAABB = new AxisAlignedBoundingBox(
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
            //AxisAlignedBoundingBoxes[i] = currentAABB;
            //AxisAlignedBoundingBoxes[i + 1] = currentAABB;
            //AxisAlignedBoundingBoxes[i + 2] = currentAABB;
        }
        var allVertices = LowerVertices.Select(v => new Vector3(v.X, v.Y, v.Z)).ToList();
        allVertices.AddRange(UpperVertices);
        OverallAxisAlignedBoundingBox = new AxisAlignedBoundingBox([.. allVertices]);
    }

    public bool PointIsWithinAnyVolume(Vector3 point)
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
                        return true;
                    }
                }
            }
        }
        return false;
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

    private void DetermineCollisonPlanes()
    {
        _compareIntersectX = new Func<double, double, bool>[LowerVertices.Length];
        _compareIntersectY = new Func<double, double, bool>[LowerVertices.Length];
        for (var i = 0; i < LowerVertices.Length; i += 3)
        {
            var currentToNextX = LowerVertices[i + 1].X - LowerVertices[i].X;
            var currentToNextY = LowerVertices[i + 1].Y - LowerVertices[i].Y;
            var pointFactorX = (LowerVertices[i + 2].X - LowerVertices[i].X) / currentToNextX;
            var pointFactorY = (LowerVertices[i + 2].Y - LowerVertices[i].Y) / currentToNextY;
            var axisIntersectX = LowerVertices[i].X + (currentToNextX * pointFactorY);
            var axisIntersectY = LowerVertices[i].Y + (currentToNextY * pointFactorX);

            _compareIntersectX[i] = LowerVertices[i + 2].X < axisIntersectX
                ? CompareLess
                : CompareGreater;

            _compareIntersectY[i] = LowerVertices[i + 2].Y < axisIntersectY
                ? CompareLess
                : CompareGreater;

            currentToNextX = LowerVertices[i + 2].X - LowerVertices[i + 1].X;
            currentToNextY = LowerVertices[i + 2].Y - LowerVertices[i + 1].Y;
            pointFactorX = (LowerVertices[i].X - LowerVertices[i + 1].X) / currentToNextX;
            pointFactorY = (LowerVertices[i].Y - LowerVertices[i + 1].Y) / currentToNextY;
            axisIntersectX = LowerVertices[i + 1].X + (currentToNextX * pointFactorY);
            axisIntersectY = LowerVertices[i + 1].Y + (currentToNextY * pointFactorX);

            _compareIntersectX[i + 1] = LowerVertices[i].X < axisIntersectX
                ? CompareLess
                : CompareGreater;

            _compareIntersectY[i + 1] = LowerVertices[i].Y < axisIntersectY
                ? CompareLess
                : CompareGreater;

            currentToNextX = LowerVertices[i].X - LowerVertices[i + 2].X;
            currentToNextY = LowerVertices[i].Y - LowerVertices[i + 2].Y;
            pointFactorX = (LowerVertices[i + 1].X - LowerVertices[i + 2].X) / currentToNextX;
            pointFactorY = (LowerVertices[i + 1].Y - LowerVertices[i + 2].Y) / currentToNextY;
            axisIntersectX = LowerVertices[i + 2].X + (currentToNextX * pointFactorY);
            axisIntersectY = LowerVertices[i + 2].Y + (currentToNextY * pointFactorX);

            _compareIntersectX[i + 2] = LowerVertices[i + 1].X < axisIntersectX
                ? CompareLess
                : CompareGreater;

            _compareIntersectY[i + 2] = LowerVertices[i + 1].Y < axisIntersectY
                ? CompareLess
                : CompareGreater;
        }
    }

    private void DetermineCollisonPlanes(int v0Index, int v1Index, int v2Index)
    {
        var currentToNextX = LowerVertices[v0Index].X - LowerVertices[v1Index].X;
        var currentToNextY = LowerVertices[v0Index].Y - LowerVertices[v1Index].Y;
        var pointFactorX = (LowerVertices[v2Index].X - LowerVertices[v1Index].X) / currentToNextX;
        var pointFactorY = (LowerVertices[v2Index].Y - LowerVertices[v1Index].Y) / currentToNextY;
        var axisIntersectX = LowerVertices[v1Index].X + (currentToNextX * pointFactorY);
        var axisIntersectY = LowerVertices[v1Index].Y + (currentToNextY * pointFactorX);

        _compareIntersectX[v0Index] = LowerVertices[v2Index].X < axisIntersectX
            ? CompareLess
            : CompareGreater;

        _compareIntersectY[v0Index] = LowerVertices[v2Index].Y < axisIntersectY
            ? CompareLess
            : CompareGreater;
    }

    public LineListPrimitive GetLineListPrimitive(GraphicsDevice graphicsDevice)
    {
        var volumeLineVertices = new List<Vector3>();
        for (int i = 0; i < VertexCount; i += 3)
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
        var normalLineColours = volumeLineVertices
            .Select(x => new Color(1.0F, 0.0F, 0.0F)).ToArray();

        return new LineListPrimitive(graphicsDevice, [.. volumeLineVertices], normalLineColours);
    }
}