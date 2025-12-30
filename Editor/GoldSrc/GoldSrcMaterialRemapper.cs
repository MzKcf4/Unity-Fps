using Mono.CSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using System.Drawing.Imaging;

public class GoldSrcImageBlueColorConvertor
{
    private static readonly string WAD_FOLDER_ROOT = "FpsResources\\Maps\\_GoldSrcWad";

    [MenuItem("Assets/GoldSrc/Remap textures")]
    public static void RemapTextures()
    {
        var selectedObjects = Selection.objects;
        foreach (var selectedObject in selectedObjects)
        {
            DoRemap(selectedObject, GetExistingTextures());
        }

        AssetDatabase.Refresh();
    }

    private static void DoRemap(UnityEngine.Object selectedObject , Dictionary<string,string> existingTextures)
    {
        string fileName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(selectedObject));
        if (selectedObject is not Material)
        {
            Debug.LogError("Only .mat file is supported");
            return;
        }

        var material = selectedObject as Material;
        var matching = existingTextures[fileName.ToLower()];
        if (matching == null) 
        { 
            Debug.LogError("No matching texture found for material: " + fileName);
            return;
        }
        Debug.Log("Remapping material: " + fileName + " to texture at: " + matching);
        material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(matching));
    }

    private static Dictionary<string, string> GetExistingTextures()
    {
        string WAD_FOLDER_ROOT_FULL = Path.Combine(Application.dataPath, WAD_FOLDER_ROOT);
        DirectoryInfo dirInfo = new DirectoryInfo(WAD_FOLDER_ROOT_FULL);
        FileInfo[] animationFileInfos = dirInfo.GetFiles("*.png", SearchOption.AllDirectories);
        Dictionary<string, string> existingTextures = new Dictionary<string, string>();
        foreach (var fileInfo in animationFileInfos)
        {
            var fileNameNoExt = Path.GetFileNameWithoutExtension(fileInfo.Name);
            var assetPath = FpsEditorUtils.ConvertToAssetPath(fileInfo.FullName);
            existingTextures[fileNameNoExt.ToLower()] = assetPath;
            Debug.Log("Added to dict: " + existingTextures[fileNameNoExt.ToLower()] + " at " +  assetPath);
        }

        return existingTextures;
    }
}

