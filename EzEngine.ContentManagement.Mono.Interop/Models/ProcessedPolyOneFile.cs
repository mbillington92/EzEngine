using Microsoft.Xna.Framework;
using EzEngine.ContentManagement.Models.PolyOneFile;
using EzEngine.ContentManagement.Mono.Interop.Enums;

namespace EzEngine.ContentManagement.Mono.Interop.Models;

public class ProcessedPolyOneFile
{
    public MetaData PolyOneMeta { get; set; } = null!;
    public ProcessedPolyOneFilePrimitiveGroup[] PrimitiveGroups { get; set; } = null!;
    public ProcessedPolyOneFilePointLightSet[]? PointLights { get; private set; }
    public ProcessedPolyOneFileVolumeSet[]? Volumes { get; private set; }
    public Dictionary<string, ProcessedPolyOneFileCustomProperty>? FileCustomProperties { get; private set; }
    public Vector3 DirectionalLightVector { get; private set; }
    public Color DirectionalLightColour { get; private set; }
    public Color AmbientLightColour { get; private set; }
    public decimal FileVersion { get; private set; }
    public string OriginalFileName { get; private set; }

    /// <summary>
    /// The location of the primitive's vertices with the lowest numeric value on the X and Y axes
    /// </summary>
    public Vector2 Min { get; private set; }
    /// <summary>
    /// The location of the primitive's vertices with the highest numeric value on the X and Y axes
    /// </summary>
    public Vector2 Max { get; private set; }
    /// <summary>
    /// The local distance between the first and last vertices of the primitive on the X and Y axes
    /// </summary>
    public Vector2 Dimensions { get; private set; }
    private bool OffsetIsRemoved = false;
    /// <summary>
    /// Names of primitives that we DON'T typically want render during normal gameplay
    /// </summary>
    public string[] NonRenderablePrimitiveFilter { get; private set; }

    public ProcessedPolyOneFile(PolyOneRawFileData rawFileData)
    {
        NonRenderablePrimitiveFilter =
        [
            "Volumes",
            "PointLights",
            "Dummy" //Use layers/primitives named this to influence the min/max dimensions/AABB calculation without using actual geometry in practice
        ];

        PolyOneMeta = rawFileData.PolyOneMeta;
        OriginalFileName = rawFileData.FileName;
        FileVersion = decimal.Parse(rawFileData.PolyOneMeta.FileVersion);

        PointLights = rawFileData.LayerGroups.SelectMany(x => x.Layers)
            .Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals("PointLights", StringComparison.CurrentCultureIgnoreCase))
            .Select(x => new ProcessedPolyOneFilePointLightSet(x, this))
            .ToArray();

        PrimitiveGroups = rawFileData.LayerGroups
            .Select(x => new ProcessedPolyOneFilePrimitiveGroup(x, this))
            .ToArray();

        Volumes = rawFileData.LayerGroups.SelectMany(x => x.Layers)
            .Where(x => x.Name == "Volumes" && x.VertexCount > 0)
            .Select(x => new ProcessedPolyOneFileVolumeSet(x, this))
            .ToArray();
        
        var minX = float.MaxValue;
        var minY = float.MaxValue;
        var maxX = float.MinValue;
        var maxY = float.MinValue;
        foreach (var primitiveGroup in PrimitiveGroups)
        {
            foreach (var primitive in primitiveGroup.Primitives)
            {
                for (int i = 0; i < primitive.VertexCount; i++)
                {
                    minX = (int)Math.Min(primitive.VertexPositions[i].X, minX);
                    minY = (int)Math.Min(primitive.VertexPositions[i].Y, minY);
                    maxX = (int)Math.Max(primitive.VertexPositions[i].X, maxX);
                    maxY = (int)Math.Max(primitive.VertexPositions[i].Y, maxY);
                }
            }
        }
        Min = new Vector2(minX, minY);
        Max = new Vector2(maxX, maxY);
        Dimensions = new Vector2(Max.X - Min.X, Max.Y - Min.Y);

        FileCustomProperties = rawFileData.CustomProperties.Levels
            .Select((x, index) => new ProcessedPolyOneFileCustomProperty
            (
                rawFileData.CustomProperties.InternalNames[index],
                rawFileData.CustomProperties.FriendlyNames[index],
                (CustomPropertyType)rawFileData.CustomProperties.Types[index],
                (CustomPropertyLevel)rawFileData.CustomProperties.Levels[index],
                rawFileData.CustomProperties.DefaultValues[index]
            ))
            .Where(x => x.Level == (int)CustomPropertyLevel.File)
            .ToDictionary(x => x.InternalName, y => y);
        
        DirectionalLightVector = new Vector3(-256.0F, -192.0F, 176.0F);
        DirectionalLightColour = new Color(1.0F, 1.0F, 1.0F);
        AmbientLightColour = new Color(0.3F, 0.3F, 0.3F);

        if (FileCustomProperties.Keys.Any(x => x == "DirectionalLightR") &&
            FileCustomProperties.Keys.Any(x => x == "DirectionalLightG") &&
            FileCustomProperties.Keys.Any(x => x == "DirectionalLightB"))
        {
            DirectionalLightColour = new Color(
                Convert.ToSingle(FileCustomProperties["DirectionalLightR"].DefaultValue),
                Convert.ToSingle(FileCustomProperties["DirectionalLightG"].DefaultValue),
                Convert.ToSingle(FileCustomProperties["DirectionalLightB"].DefaultValue));
        }
        if (FileCustomProperties.Keys.Any(x => x == "AmbientLightR") &&
            FileCustomProperties.Keys.Any(x => x == "AmbientLightG") &&
            FileCustomProperties.Keys.Any(x => x == "AmbientLightB"))
        {
            AmbientLightColour = new Color(
                Convert.ToSingle(FileCustomProperties["AmbientLightR"].DefaultValue),
                Convert.ToSingle(FileCustomProperties["AmbientLightG"].DefaultValue),
                Convert.ToSingle(FileCustomProperties["AmbientLightB"].DefaultValue));
        }
        if (FileCustomProperties.Keys.Any(x => x == "LightVectorX") &&
            FileCustomProperties.Keys.Any(x => x == "LightVectorY") &&
            FileCustomProperties.Keys.Any(x => x == "LightVectorZ"))
        {
            DirectionalLightVector = new Vector3(
                Convert.ToSingle(FileCustomProperties["LightVectorX"].DefaultValue),
                Convert.ToSingle(FileCustomProperties["LightVectorY"].DefaultValue),
                Convert.ToSingle(FileCustomProperties["LightVectorZ"].DefaultValue));
        }
    }

    /// <summary>
    /// Moves the top left corner of the whole contents to 0,0 taking ALL primitives in the file into consideration.
    /// </summary>
    public void RemoveOffset()
    {
        if (OffsetIsRemoved) return;
        foreach (var primitiveGroup in PrimitiveGroups)
        {
            foreach (var primitive in primitiveGroup.Primitives)
            {
                //Because the X axis is inverted in MonoGame, we subtract the maximum rather than the minimum
                primitive.VertexPositions = primitive.VertexPositions
                    .Select(v => new Vector3(v.X - Max.X, v.Y - Min.Y, v.Z))
                    .ToArray();
            }
        }
        foreach (var volumeSet in Volumes)
        {
            volumeSet.RemoveOffset(Min, Max);
        }
        foreach (var pointLightSet in PointLights)
        {
            pointLightSet.RemoveOffset(Min, Max);
        }
        OffsetIsRemoved = true;
    }

    /// <summary>
    /// Applies transformations to all base primitives belonging to this file based on an input triangle
    /// </summary>
    /// <param name="v0">Determines: The "root vertex" the contents are projected from along the local X and Y axis; the offset of the contents from 0,0</param>
    /// <param name="v1">Determines: the rotation of the contents based on the angle from v0 to this vertex; the scale of the contents along the local X axis based on its distance as a multiple of the original contents width; the distance to skew the contents along the Z axis based on local X axis distance from v0</param>
    /// <param name="v2">Determines: the scale of the contents along the local Y axis, based on its distance as a multiple of the original contents width; the distance to skew the contents along the Z axis based on local Y axis distance from v0</param>
    public void ApplyTransformation(Vector3 v0, Vector3 v1, Vector3 v2)
    {
        var modelInstanceWidth = Math.Sqrt(Helpers.DistanceSquared(v0.X, v0.Y, v1.X, v1.Y));
        var modelXScale = 1.0D;
        if (Dimensions.X != 0.0D)
        {
            modelXScale = modelInstanceWidth / Dimensions.X;
        }
        var modelInstanceHeight = Math.Sqrt(Helpers.DistanceSquared(v0.X, v0.Y, v2.X, v2.Y));
        var modelYScale = 1.0D;
        if (Dimensions.Y != 0.0D)
        {
            modelYScale = modelInstanceHeight / Dimensions.Y;
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

        foreach (var primitiveGroup in PrimitiveGroups)
        {
            primitiveGroup.ApplyTransformation(v0, sine, cosine, new Vector3(
                (float)modelXScale, (float)modelYScale, 1.0F
            ), normal);
        }
        foreach (var volumeSet in Volumes)
        {
            volumeSet.ApplyTransformation(v0, sine, cosine, new Vector3(
                (float)modelXScale, (float)modelYScale, 1.0F
            ), normal);
        }
    }

    public void CalculateVertexSurfaceNormals()
    {
        foreach (var primitiveGroup in PrimitiveGroups)
        {
            foreach (var primitive in primitiveGroup.Primitives)
            {
                primitive.VertexSurfaceNormals = Helpers.CalculateSurfaceNormals(primitive.VertexPositions);
            }
        }
    }

    public void CalculateLighting(ProcessedPolyOneFileVolumeSet[]? volumesToConsider = null, ProcessedPolyOneFilePointLightSet[]? pointLightsToConsider = null)
    {
        var volumes = volumesToConsider ?? Volumes;
        var pointLights = pointLightsToConsider ?? PointLights;
        foreach (var primitiveGroup in PrimitiveGroups)
        {
            //TODO: Make ambient light colour configurable
            primitiveGroup.CalculateLighting(pointLights, volumes, DirectionalLightVector, DirectionalLightColour, new Color(0.15F, 0.175F, 0.2F));
        }
    }
}