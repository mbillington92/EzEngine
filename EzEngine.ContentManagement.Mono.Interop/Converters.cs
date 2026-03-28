using Microsoft.Xna.Framework;
using EzEngine.ContentManagement.Mono.Interop.Models;

namespace EzEngine.ContentManagement.Mono.Interop;

public static class Converters
{
    private static Dictionary<string, ProcessedPolyOneFile> _processedFiles = [];

    /// <summary>
    /// Loads a raw PolyOne JSV file, converts it a more user-friendly object using MonoGame structs and returns it.  
    /// </summary>
    /// <returns></returns>
    public static ProcessedPolyOneFile? LoadConvertedPolyOneFile(string filePath)
    {
        if (!_processedFiles.TryGetValue(filePath, out ProcessedPolyOneFile? result))
        {
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                var rawFile = PolyOneFileLoader.GetFileData(filePath);
                result = new ProcessedPolyOneFile(rawFile);
            }
        }
        return result;
    }

    /// <summary>
    /// Convert three float lists to a <see cref="Vector3"/> array, frequently used to represent vertex co-ordinates.
    /// Typical usage: Convert a raw PolyOne file Layer's VertsX, VertsY and Z custom vertex properties. 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public static Vector3[] ConvertToVector3s(List<float> x, List<float> y, List<float> z)
    {
        var result = new List<Vector3>();
        for (var i = 0; i < x.Count; i++)
        {
            result.Add(new Vector3(x[i], y[i], z[i]));
        }
        return [.. result];
    }

    /// <summary>
    /// Convert three float arrays to a <see cref="Vector3"/> array, frequently used to represent vertex co-ordinates.
    /// Typical usage: Convert a raw PolyOne file Layer's VertsX, VertsY and Z custom vertex properties. 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public static Vector3[] ConvertToVector3s(float[] x, float[] y, float[] z)
    {
        var result = new Vector3[x.Length];
        for (var i = 0; i < x.Length; i++)
        {
            result[i] = new Vector3(x[i], y[i], z[i]);
        }
        return result;
    }

    public static Vector2[] ConvertToVector2(List<float> x, List<float> y)
    {
        var result = new List<Vector2>();
        for (var i = 0; i < x.Count; i++)
        {
            result.Add(new Vector2(x[i], y[i]));
        }
        return [.. result];
    }

    /// <summary>
    /// Converts a list of RGB hex colours to an array of <see cref="Color"/>.
    /// Prepends a pound/hash symbol #, so this must not be included on the input list.
    /// </summary>
    /// <param name="hexColours"></param>
    /// <returns></returns>
    public static Color[] ConvertFromHex(List<string> hexColours)
    {
        var result = new List<Color>();
        foreach (var colour in hexColours)
        {
            var asSystemDrawingColor = System.Drawing.ColorTranslator.FromHtml($"#{colour}");
            result.Add(new Color(
                asSystemDrawingColor.R,
                asSystemDrawingColor.G,
                asSystemDrawingColor.B, 
                Byte.MaxValue));
        }
        return [.. result];
    }

    /// <summary>
    /// PolyOne file versions 0.1 and before (see the metadata section) save hex colours incorrectly.
    /// This should only be used on files of these versions.
    /// </summary>
    /// <param name="hexColour"></param>
    /// <returns></returns>
    public static Color ConvertFromHexLegacy(string hexColour)
    {
        //https://www.geeksforgeeks.org/dsa/program-for-hexadecimal-to-decimal/
        uint base1 = 1;
        uint colourInDecimal = 0;
        for (var i = 0; i < hexColour.Length; i += 1) 
        {
            if (hexColour[i] >= '0' &&
                hexColour[i] <= '9') 
            {
                colourInDecimal += (uint)(hexColour[i] - 48) * base1;
                base1 *= 16;
            }
            else if (hexColour[i] >= 'A' &&
                hexColour[i] <= 'F') 
            {
                colourInDecimal += (uint)(hexColour[i] - 55) * base1;
                base1 *= 16;
            }
        }
        //https://stackoverflow.com/questions/35893902/converting-msaccess-colors-to-rgb-in-vba
        //Didn't need this in the end
        /*
        var b = Math.Floor(colourInDecimal / (double)(256*256));
        var g = Math.Floor((colourInDecimal - b*256*256)/256.0D);
        var r = colourInDecimal - b*256*256 - g*256;
        */
        return new Color(colourInDecimal);
    }

    /// <summary>
    /// PolyOne JSV file versions 0.1 and before (see the metadata section) save hex colours incorrectly.
    /// This should only be used on files of these versions.
    /// </summary>
    /// <param name="hexColours"></param>
    /// <returns></returns>
    public static Color[] ConvertFromHexLegacy(List<string> hexColours)
    {
        return hexColours.Select(ConvertFromHexLegacy)
            .ToArray();
    }
}
