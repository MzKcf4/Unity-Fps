using System.Collections.Generic; 
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.IO;

public static class MapImportHelper
{
    private static readonly string CSGO_MATERIAL_PATH_IN_ASSET = "FpsResources/Entities/Maps/_csgo_shared_materials/";

    [MenuItem("Assets/Map - Set Materials")]
    public static void Test()
    {
        Object obj = Selection.activeObject;
        if (obj == null)
            return;
        string assetPath = AssetDatabase.GetAssetPath(obj);
        ModelImporter modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;
        if (modelImporter == null)
            return;

        var sourceMaterials = typeof(ModelImporter)
        .GetProperty("sourceMaterials", BindingFlags.NonPublic | BindingFlags.Instance)?
        .GetValue(modelImporter) as AssetImporter.SourceAssetIdentifier[];
        foreach (var identifier in sourceMaterials ?? Enumerable.Empty<AssetImporter.SourceAssetIdentifier>())
        {
            Material material = FindMaterialFile(identifier.name);
            if (material != null)
                modelImporter.AddRemap(identifier, material);
            else
                Debug.LogWarning("Material not found : " + identifier.name);
        }
    }

    // ar_dizzy_dizzy_wooden_planks_01
    private static Material FindMaterialFile(string materialName)
    {
        Debug.Log("Finding material for " + materialName);

        if (materialName.StartsWith("materials_")) {
            materialName = materialName.Replace("materials_", "");
        }

        List<string> nameParts = materialName.Split('_').ToList();
        if (nameParts[0].StartsWith("~")) {
            nameParts[0] = "models";
            nameParts.Insert(1, "props");
        }

        // Full csgo material folder file system path
        string materialFolderFileSystemPath = Path.Combine( Application.dataPath , CSGO_MATERIAL_PATH_IN_ASSET);

        string currentValidFolderPath = materialFolderFileSystemPath;
        int missCount = 0;
        for (int i = 0; i < nameParts.Count; i++)
        {
            // The possible folder/file name
            string nameToTest = "";
            for (int j = missCount; j >= 0; j--)
            {
                if (j < missCount)
                    nameToTest += "_";

                nameToTest += nameParts[i - j];
            }

            // Start by searching "ar" , then "ar_dizzy" , then "ar_dizzy_dizzy" ... in csgo material folder
            string pathToTest = Path.Combine(currentValidFolderPath, nameToTest);

            Debug.Log("Checking path : " + pathToTest);
            // Check if it's a directory
            if (Directory.Exists(pathToTest))
            {
                // if exists , update the current directory pointer and test the next folder.
                currentValidFolderPath = Path.Combine(currentValidFolderPath, nameToTest);
                // -1 because 'i' will increase by 1 in for loop !
                if (missCount > 0)
                    i += missCount - 1;

                missCount = 0;
            }
            else if (isMaterialFilePath(pathToTest))
            {
                // Found the material file , return it.
                string materialPath = pathToTest + ".mat";
                Material material = AssetDatabase.LoadAssetAtPath(FpsEditorUtils.ConvertToAssetPath(materialPath), typeof(Material)) as Material;
                return material;
            }
            else if (isTextureFilePath(pathToTest))
            {
                // Then check if it's Texture path , create the material for it , then return.
                string texturePath = pathToTest + ".png";
                Material material = MaterialCreateHelper.CreateMaterialForTexture(texturePath, true);
                return material;
            }
            else
            {
                missCount++;
                // if not exist , combine current one with next one and test again
                // e.g  ar ->  ar_dizzy
            }
        }
        return null;
    }

    private static bool isMaterialFilePath(string fileSystemPath)
    {
        return File.Exists(fileSystemPath + ".mat");
    }

    private static bool isTextureFilePath(string fileSystemPath)
    {
        return File.Exists(fileSystemPath + ".png");
    }

    /*
    SerializedObject modelImporterObj = new SerializedObject(modelImporter);
        SerializedProperty materials = modelImporterObj.FindProperty("m_Materials");

        for (int i = 0; i < materials.arraySize; i++)
        {
            SerializedProperty id = materials.GetArrayElementAtIndex(i);
            var name = id.FindPropertyRelative("name").stringValue;
            var type = id.FindPropertyRelative("type").stringValue;
            var assembly = id.FindPropertyRelative("assembly").stringValue;
            Debug.Log(name + " , " + type + " , " + assembly);
        }
    }
    */
}

