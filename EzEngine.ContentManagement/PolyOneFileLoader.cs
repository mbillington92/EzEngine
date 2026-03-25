using EzEngine.ContentManagement.Models.PolyOneFile;
using ServiceStack;

namespace EzEngine.ContentManagement;

public static class PolyOneFileLoader
{
    private static Dictionary<string, PolyOneRawFileData> _loadedFiles = [];

    /// <summary>
    /// Loads a PolyOne JSV file.
    /// Not recommended for direct usage as various parts of its data may not be in a useful format without further processing.
    /// Consider using an interop library if one is available for your toolset.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static PolyOneRawFileData? GetFileData(string filePath)
    {
        if (!_loadedFiles.TryGetValue(filePath, out PolyOneRawFileData? result))
        {
            if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
            {
                var fileData = File.ReadAllText(filePath);
                fileData = fileData.Replace("\r\n", "").Replace("\t", "").Replace("    ", "");
                result = fileData.FromJsv<PolyOneRawFileData>();
                result.FileName = filePath;
                _loadedFiles.Add(filePath, result);
            }
        }
        return result;
    }
}
