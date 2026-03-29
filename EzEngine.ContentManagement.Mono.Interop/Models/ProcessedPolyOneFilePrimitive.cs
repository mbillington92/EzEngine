using Microsoft.Xna.Framework;
using EzEngine.ContentManagement.Models.PolyOneFile;

namespace EzEngine.ContentManagement.Mono.Interop.Models;

public class ProcessedPolyOneFilePrimitive
{
    public string Name { get; set; } = null!;
    public string TextureName { get; set; } = null!;
    public Vector3[] VertexPositions { get; set; } = null!;
    public Vector3[]? VertexSurfaceNormals { get; set; } = null!;
    public Vector2[] VertexTextureCoordinates { get; set; } = null!;
    public Color[] VertexBaseColours { get; set; } = null!;
    public Color[] LitVertexColours { get; set; } = null!;
    public int VertexCount { get; set; }
    public Dictionary<string, CustomVertexProperty> CustomVertexProperties { get; set; } = null!;
    public readonly ProcessedPolyOneFilePrimitiveGroup Parent;

    /// <summary>
    /// Constructor to map from raw PolyOne layer to a friendlier format for MonoGame that uses their structures.
    /// </summary>
    /// <param name="rawLayer"></param>
    public ProcessedPolyOneFilePrimitive(Layer rawLayer, ProcessedPolyOneFilePrimitiveGroup parent)
    {
        Parent = parent;
        Name = rawLayer.Name;
        TextureName = rawLayer.TextureName;

        var zPropertyIndex = rawLayer.CustomVertexProperties
            .FindIndex(x => x.Name.Equals("z", StringComparison.CurrentCultureIgnoreCase));
        var z = zPropertyIndex >= 0
            ? rawLayer.CustomVertexProperties[zPropertyIndex].Values
                .Select(float.Parse)
                .ToList()
            : rawLayer.VertsX
                .Select(x => 0.0F)
                .ToList();

        //Invert X axis vertex positions, because +X in PolyOne is -X in MonoGame and vice versa
        var vertsXFlipped = rawLayer.VertsX
            .Select(x => -x)
            .ToList();

        VertexPositions = Converters.ConvertToVector3s(vertsXFlipped, rawLayer.VertsY, z);
        VertexCount = VertexPositions.Length;
        VertexTextureCoordinates = Converters.ConvertToVector2(rawLayer.VertsXTex, rawLayer.VertsYTex);
        VertexBaseColours = Parent.Parent.FileVersion <= 0.1M
            ? Converters.ConvertFromHexLegacy(rawLayer.VertsColour)
            : Converters.ConvertFromHex(rawLayer.VertsColour);
        LitVertexColours = new Color[VertexCount];
        VertexSurfaceNormals = Helpers.CalculateSurfaceNormals(VertexPositions);
        CustomVertexProperties = rawLayer.CustomVertexProperties
            .ToDictionary(x => x.Name, x => x);
    }

    /// <summary>
    /// Applies a transformation to all vertices in this primitive
    /// </summary>
    /// <param name="offset">Number of units to offset the primitive by in the X, Y and Z axes</param>
    /// <param name="zRotation">Z Rotation in radians to apply to the primitive, relative to the top left corner of the file this primitive belongs to</param>
    /// <param name="nonUniformScale">X, Y and Z scaling</param>
    /// <param name="skewNormal">Normal determining what angle to skew the tris along the Z axis based on distance from the top left corner</param>
    public void ApplyTransformation(Vector3 offset, double sine, double cosine, Vector3 nonUniformScale, Vector3 skewNormal)
    {
        for (int i = 0; i < VertexPositions.Length; i++)
        {
            var initialPosition = new Vector3(VertexPositions[i].X, VertexPositions[i].Y, VertexPositions[i].Z);
            VertexPositions[i].X = (float)(offset.X + initialPosition.X * nonUniformScale.X * cosine + initialPosition.Y * nonUniformScale.Y * -sine);
            VertexPositions[i].Y = (float)(offset.Y + initialPosition.Y * nonUniformScale.Y * cosine + initialPosition.X * nonUniformScale.X * sine);

            var v0x = offset.X;
            var v0y = offset.Y;
            var v0z = offset.Z;
            var nX = skewNormal.X;
            var nY = skewNormal.Y;
            var nZ = skewNormal.Z;
            var skewTargetZ = v0z - ((VertexPositions[i].X - v0x) * nX + (VertexPositions[i].Y - v0y) * nY) / nZ;
            VertexPositions[i].Z = initialPosition.Z + skewTargetZ;
        }
        VertexSurfaceNormals = Helpers.CalculateSurfaceNormals(VertexPositions);
    }

    public void ApplyTransformation(Vector3 v0, Vector3 v1, Vector3 v2)
    {
        var modelInstanceWidth = Math.Sqrt(Helpers.DistanceSquared(v0.X, v0.Y, v1.X, v1.Y));
        var modelXScale = 1.0D;
        var modelBaseWidth = Parent.Parent.Dimensions.X;
        if (modelBaseWidth != 0.0D)
        {
            modelXScale = modelInstanceWidth / modelBaseWidth;
        }
        var modelInstanceHeight = Math.Sqrt(Helpers.DistanceSquared(v0.X, v0.Y, v2.X, v2.Y));
        var modelYScale = 1.0D;
        var modelBaseHeight = Parent.Parent.Dimensions.Y;
        if (modelBaseHeight != 0.0D)
        {
            modelYScale = modelInstanceHeight / modelBaseHeight;
        }
        var modelRotationRadians = Helpers.PointDirection(v0.X, v0.Y, v1.X, v1.Y);
        var sine = Math.Sin(modelRotationRadians);
        var cosine = Math.Cos(modelRotationRadians);

        Vector3[] tri =
        [
            v0,
            v1,
            v2
        ];
        var normal = Helpers.CalculateSurfaceNormals(tri)[0];

        ApplyTransformation(v0, sine, cosine, new Vector3(
            (float)modelXScale,
            (float)modelYScale,
            1.0F
        ), normal);
    }

    public void CalculateLighting(ProcessedPolyOneFilePointLightSet[] pointLightSets, ProcessedPolyOneFileVolumeSet[] volumeSets, Vector3 directionalLightVector, Color directionalLightColour, Color ambientLightColour)
    {
        var maxNonAmbientLightContrib = new Color(255 - ambientLightColour.R, 255 - ambientLightColour.G, 255 - ambientLightColour.B);
        var directionalLightBaseFactor = new Color(
            directionalLightColour.R / 255 * maxNonAmbientLightContrib.R,
            directionalLightColour.G / 255 * maxNonAmbientLightContrib.G,
            directionalLightColour.B / 255 * maxNonAmbientLightContrib.B
        );

        var directionalLightNormal = new Vector3(directionalLightVector.X, directionalLightVector.Y, directionalLightVector.Z);
        directionalLightNormal.Normalize();
        for (var i = 0; i < VertexCount; i += 3)
        {
            var dotProduct = VertexSurfaceNormals[i].X * directionalLightNormal.X
                + VertexSurfaceNormals[i].Y * directionalLightNormal.Y
                + VertexSurfaceNormals[i].Z * directionalLightNormal.Z;
            var directionalLightFactor = Math.Max(dotProduct, 0.0F);

            for (var j = 0; j < 3; j++)
            {
                //We can probably get away with not calculating lighting if this vertex already has a very dark base colour.
                if (VertexBaseColours[i + j].R < 3 && VertexBaseColours[i + j].G < 3 && VertexBaseColours[i + j].B < 3)
                {
                    continue;
                }
                var nonAmbientLightR = 0.0F;
                var nonAmbientLightG = 0.0F;
                var nonAmbientLightB = 0.0F;
                foreach (var pointLightSet in pointLightSets)
                {
                    for (int k = 0; k < pointLightSet.PointLightCount; k++)
                    {
                        //Don't use the helper for squared distance in this case because we potentially have to reuse
                        //the manhattan distance for the direction normal to the light
                        var xDistance = pointLightSet.Positions[k].X - VertexPositions[i + j].X;
                        var yDistance = pointLightSet.Positions[k].Y - VertexPositions[i + j].Y;
                        var zDistance = pointLightSet.Positions[k].Z - VertexPositions[i + j].Z;
                        var distance = new Vector3(xDistance, yDistance, zDistance);
                        var distanceSquared = xDistance * xDistance + yDistance * yDistance + zDistance * zDistance;
                        if (distanceSquared <= pointLightSet.FalloffDistance[k] * pointLightSet.FalloffDistance[k]
                            && !Helpers.BinaryRaycast(volumeSets, 4.0F, VertexPositions[i + j], distance))
                        {
                            var trueDistance = Math.Sqrt(distanceSquared);

                            var pointLightDirectionNormalX = xDistance / trueDistance;
                            var pointLightDirectionNormalY = yDistance / trueDistance;
                            var pointLightDirectionNormalZ = zDistance / trueDistance;

                            var pointLightDotProduct = VertexSurfaceNormals[i + j].X * pointLightDirectionNormalX
                                + VertexSurfaceNormals[i + j].Y * pointLightDirectionNormalY
                                + VertexSurfaceNormals[i + j].Z * pointLightDirectionNormalZ;

                            var directionBasedIntensity = Math.Max(pointLightDotProduct, 0.0F);
                            var distanceBasedIntensity = Math.Max(1 - trueDistance / pointLightSet.FalloffDistance[k], 0.0F);
                            nonAmbientLightR += (float)(directionBasedIntensity * distanceBasedIntensity * pointLightSet.Colours[k].R);
                            nonAmbientLightG += (float)(directionBasedIntensity * distanceBasedIntensity * pointLightSet.Colours[k].G);
                            nonAmbientLightB += (float)(directionBasedIntensity * distanceBasedIntensity * pointLightSet.Colours[k].B);
                        }
                    }
                }
                //If there's a collisison between the vertex and directional light, then don't apply directional light
                if (!Helpers.BinaryRaycast(volumeSets, 10.0F, VertexPositions[i + j], directionalLightVector * 4.0F))
                {
                    nonAmbientLightR += directionalLightBaseFactor.R * directionalLightFactor;
                    nonAmbientLightG += directionalLightBaseFactor.G * directionalLightFactor;
                    nonAmbientLightB += directionalLightBaseFactor.B * directionalLightFactor;
                }

                LitVertexColours[i + j] = new Color(
                    (VertexBaseColours[i + j].R * ambientLightColour.R / 255 + Math.Min(nonAmbientLightR, maxNonAmbientLightContrib.R)) / 255,
                    (VertexBaseColours[i + j].G * ambientLightColour.G / 255 + Math.Min(nonAmbientLightG, maxNonAmbientLightContrib.G)) / 255,
                    (VertexBaseColours[i + j].B * ambientLightColour.B / 255 + Math.Min(nonAmbientLightB, maxNonAmbientLightContrib.B)) / 255
                );
            }
        }
    }

    public Vector3[] GetCentroids(Vector3[] vertexPositions)
    {
        var result = new Vector3[vertexPositions.Length];
        for (int i = 0; i < vertexPositions.Length; i += 3)
        {
            var halfDist = new Vector3(
                (vertexPositions[i + 2].X - vertexPositions[i + 1].X) * 0.5F,
                (vertexPositions[i + 2].Y - vertexPositions[i + 1].Y) * 0.5F,
                (vertexPositions[i + 2].Z - vertexPositions[i + 1].Z) * 0.5F);

            var midpoint = new Vector3(
                vertexPositions[i + 1].X + halfDist.X,
                vertexPositions[i + 1].Y + halfDist.Y,
                vertexPositions[i + 1].Z + halfDist.Z);

            result[i] = new Vector3(
                vertexPositions[i].X + (midpoint.X - vertexPositions[i].X) * 0.66F,
                vertexPositions[i].Y + (midpoint.Y - vertexPositions[i].Y) * 0.66F,
                vertexPositions[i].Z + (midpoint.Z - vertexPositions[i].Z) * 0.66F
            );
            result[i + 1] = result[i];
            result[i + 2] = result[i];
        }
        return result;
    }
}