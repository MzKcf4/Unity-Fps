using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using QFSW.QC.Utilities;

public class VmtReader
{
    private static readonly string REGEX_REDUCE_MULTI_SPACE = @"[ ]{2,}";

    [MenuItem("Assets/Test/ReadVmt")]
    public static void TestParseVmt() 
    {
        Object selectedObject = Selection.activeObject;
        string assetPath = AssetDatabase.GetAssetPath(selectedObject);
        string fileSystemPath = Path.Combine(Directory.GetCurrentDirectory(), AssetDatabase.GetAssetPath(selectedObject));

        MaterialSettings materialSettings = new MaterialSettings();
        ParseFromVmt(materialSettings, fileSystemPath);
    }
    
    public static void ParseFromVmt(MaterialSettings materialSettings , string vmtFsPath)
    {
        Debug.Log("Parsing " + vmtFsPath);
        string[] fileLines = File.ReadAllLines(vmtFsPath);
        for (int i = 0; i < fileLines.Length; i++)
        {
            string line = fileLines[i];
            // Only consider texture paths
            if (line.Contains("$") && (line.Contains("/") || line.Contains("\\")))
            {
                line = line.TrimStart(' ');
                line = line.Trim();
                line = Regex.Replace(line.Replace("\t", " "), REGEX_REDUCE_MULTI_SPACE, " ");
                line = line.Replace("\"\"", "\" \"");
                line = line.Replace("\"", "");

                string[] parts = line.Split(' ');

                if (parts[0].ToLower().Contains("$basetexture"))
                {
                    string baseMapName;
                    if (parts.Length > 1)
                    {
                        var fullPath = string.Join(" ", parts, 1, parts.Length - 1);
                        baseMapName = GetLastPathSegment(fullPath);
                    }
                    else
                        baseMapName = parts[1];

                    materialSettings.baseMapName = baseMapName;
                    Debug.Log("Found base map : " + materialSettings.baseMapName);
                }
                else if (parts[0].ToLower().Contains("$bumpmap"))
                {
                    string normalMapName;
                    if (parts.Length > 1)
                    {
                        var fullPath = string.Join(" ", parts, 1, parts.Length - 1);
                        normalMapName = GetLastPathSegment(fullPath);
                    }
                    else
                        normalMapName = parts[1];

                    materialSettings.normalMapName = normalMapName;
                    // materialSettings.normalMapName = Path.GetFileNameWithoutExtension(parts[1]);
                    Debug.Log("Found normal map : " + materialSettings.normalMapName);
                }
                else if (parts[0].ToLower().Contains("$translucent"))
                {
                    materialSettings.isTransparent = true;
                }
            }
        }
    }

    public static string GetLastPathSegment(string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;

        // Normalize path separators
        path = path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);

        // Remove trailing separators
        path = path.TrimEnd(Path.DirectorySeparatorChar);

        // Get the last segment
        return Path.GetFileNameWithoutExtension(path);
    }
}
