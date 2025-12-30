using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class MaterialMapper
{
    private static readonly string VMT_FOLDER = "Vmts";
    private static readonly string TEXTURE_FOLDER = "Textures";
    private static readonly string MATERIAL_FOLDER = "Materials";


    [MenuItem("Assets/GetEmbeddedMaterials_")]
    public static void Test()
    {
        Object selectedObject = Selection.activeObject;
        string assetPath = AssetDatabase.GetAssetPath(selectedObject);

        //Find the names of embedded materials in the AssetDatabase
        IEnumerable<string> namesOfMaterialsInAssetDatabase = AssetDatabase.LoadAllAssetsAtPath(assetPath)
            .Where(x => x.GetType() == typeof(Material))
            .Select(x => x.name);

        foreach (string name in namesOfMaterialsInAssetDatabase)
        {
            Debug.Log(name);
        }
    }

    [MenuItem("Assets/AutoMapMaterials (Click fbx)")]
    public static void MapFbxMaterials()
    {
        Object selectedObject = Selection.activeObject;
        string weaponFbxPath = AssetDatabase.GetAssetPath(selectedObject);
        string ext = Path.GetFileName(AssetDatabase.GetAssetPath(selectedObject));
        if (!ext.Contains(".fbx"))
        {
            Debug.LogError("Only .fbx file is supported");
            return;
        }
        
        string currentFolderAssetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(selectedObject));
        string vmtFolderAssetPath = Path.Combine(currentFolderAssetPath, VMT_FOLDER);
        string textureFolderAssetPath = Path.Combine(currentFolderAssetPath, TEXTURE_FOLDER);
        string materialFolderAssetPath = Path.Combine(currentFolderAssetPath, MATERIAL_FOLDER);

        /*
        // First gather a list of materials required for the model
        List<MaterialSettings> fbxMaterials = ExtractFbxMaterials(weaponFbxPath, materialFolderAssetPath);
        foreach (MaterialSettings fbxMaterial in fbxMaterials)
        {
            ExtractVmtMaterialSettings(vmtFolderAssetPath, fbxMaterial.MaterialName, fbxMaterial);
            // Now we have path to texture , make material file from them
            CreateMaterial(textureFolderAssetPath, materialFolderAssetPath, fbxMaterial);
        }
        */


        // Gather a list of materials required for the model
        Dictionary<string, MaterialSettings> dictMaterialNameToSettings = MapMaterialNames(weaponFbxPath);

        // Then  read the corresponding .vmt file to load the textures
        foreach (KeyValuePair<string, MaterialSettings> kvp in dictMaterialNameToSettings)
        {
            ExtractVmtMaterialSettings(vmtFolderAssetPath, kvp.Key, kvp.Value);
            // Now we have path to texture , make material file from them
            CreateMaterial(textureFolderAssetPath, materialFolderAssetPath, kvp.Value);
        }
        
    }

    private static List<MaterialSettings> ExtractFbxMaterials(string weaponFbxPath , string materialFolderAssetPath) 
    {
        List<MaterialSettings> list = new List<MaterialSettings>();
        Object[] fbxObjects = AssetDatabase.LoadAllAssetsAtPath(weaponFbxPath);
        foreach (Object fbxObject in fbxObjects)
        {
            if (fbxObject is Material)
            {
                Material material = (Material)fbxObject;

                MaterialSettings materialSettings = new MaterialSettings()
                {
                    MaterialName = material.name,
                };
                Debug.Log("Found material : " + material.name);
                list.Add(materialSettings);
            }
        }
        return list;
    }

    private static Dictionary<string, MaterialSettings> MapMaterialNames(string weaponFbxPath)
    {
        Dictionary<string, MaterialSettings> dictMaterialNameToSettings = new Dictionary<string, MaterialSettings>();

        //Find the names of embedded materials in the fbx
        IEnumerable<string> embeddedMaterialNames = AssetDatabase.LoadAllAssetsAtPath(weaponFbxPath)
            .Where(x => x.GetType() == typeof(Material))
            .Select(x => x.name);

        foreach (string name in embeddedMaterialNames)
        {
            MaterialSettings materialSettings = new MaterialSettings();
            materialSettings.MaterialName = name;
            dictMaterialNameToSettings.Add(name, materialSettings);
            Debug.Log("Found material : " + name);
        }
            

        return dictMaterialNameToSettings;
    }

    private static void ExtractVmtMaterialSettings(string vmtFolderAssetPath, string assetMaterialName, MaterialSettings materialSettings)
    {
        // Try go to vmt folder to find corresponding vmt file
        string targetVmtName = assetMaterialName + ".vmt";
        DirectoryInfo dirInfo = new DirectoryInfo(vmtFolderAssetPath);
        FileInfo[] animationFileInfos = dirInfo.GetFiles("*.vmt", SearchOption.TopDirectoryOnly);
        bool found = false;
        foreach (FileInfo fileInfo in animationFileInfos)
        {
            if (fileInfo.FullName.ToUpper().EndsWith(targetVmtName.ToUpper()))
            {
                string vmtAssetPath = Path.Combine(vmtFolderAssetPath, fileInfo.FullName);
                VmtReader.ParseFromVmt(materialSettings, vmtAssetPath);
                found = true;
                break;
            }
        }
        if (!found) 
        {
            Debug.LogWarning("No vmt found for " + assetMaterialName);
        }
        
    }

    private static void CreateMaterial(string textureFolderAssetPath, string materialFolderAssetPath, MaterialSettings materialSettings) 
    {
        // First check if material exists already
        string materialAssetPath = Path.Combine(materialFolderAssetPath, materialSettings.MaterialName + ".mat");
        if (File.Exists(materialAssetPath)) {
            Debug.Log("Material " + materialSettings.MaterialName + " already exists");
            return;
        }

        // If not then create from texture
        MaterialCreator.CreateMaterial(textureFolderAssetPath, materialFolderAssetPath, materialSettings);
    }

    private static void UpdateMaterialFromSetting() { }
}