using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

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
                line = line.Trim();
                line = Regex.Replace(line.Replace("\t", " "), REGEX_REDUCE_MULTI_SPACE, " ");
                line = line.Replace("\"", "");

                string[] parts = line.Split(' ');

                if (parts[0].Contains("$basetexture"))
                    materialSettings.baseMapName = Path.GetFileNameWithoutExtension(parts[1]);
                else if (parts[0].Contains("$bumpmap"))
                    materialSettings.normalMapName = Path.GetFileNameWithoutExtension(parts[1]);
            }
        }
    }
}
