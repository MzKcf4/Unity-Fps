using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.IO;

public static class MaterialCreateHelper
{
    [MenuItem("Assets/Create material from textures")]
    public static void CreateMaterialBySelection()
    {
        var selectedAsset = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);

        var cnt = selectedAsset.Length * 1.0f;
        var idx = 0f;
        foreach (Object obj in selectedAsset)
        {
            idx++;
            EditorUtility.DisplayProgressBar("Create material", "Create material for: " + obj.name, idx / cnt);

            if (obj is Texture2D)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                CreateMaterialForTexture(assetPath , false);
            }
        }
        EditorUtility.ClearProgressBar();
    }

    public static Material CreateMaterialForTexture(string baseTexturePath , bool isFileSystemPath)
    {
        if(isFileSystemPath)
            baseTexturePath = FpsEditorUtils.ConvertToAssetPath(baseTexturePath);

        Texture2D baseTexture = AssetDatabase.LoadAssetAtPath(baseTexturePath, typeof(Texture2D)) as Texture2D;
        Texture2D normalMapTexture = FindNormalMapTexture(baseTexturePath);

        Material createdMaterial = CreateMatFromTx(baseTexture, normalMapTexture, Shader.Find("Universal Render Pipeline/Lit"));

        if (normalMapTexture != null)
            Debug.Log("Material created with normal map : " + baseTexturePath);
        else
            Debug.Log("Material created : " + baseTexturePath);

        return createdMaterial;
    }

    private static Texture2D FindNormalMapTexture(string baseTextureAssetPath)
    {
        string baseName = Path.GetFileNameWithoutExtension(baseTextureAssetPath);
        string directory = Path.GetDirectoryName(baseTextureAssetPath);

        string normalMapName_a = baseName + "_n.png";
        string normalMapName_b = baseName + "_normal.png";
        string normalMapName_c = baseName + "_normals.png";

        // a
        string normalMapAssetPath = Path.Combine(directory, normalMapName_a);
        Texture2D normalMapTexture = AssetDatabase.LoadAssetAtPath(normalMapAssetPath, typeof(Texture2D)) as Texture2D;

        // b
        if (normalMapTexture == null)
        {
            normalMapAssetPath = Path.Combine(directory, normalMapName_b);
            normalMapTexture = AssetDatabase.LoadAssetAtPath(normalMapAssetPath, typeof(Texture2D)) as Texture2D;
        }

        // c
        if (normalMapTexture == null)
        {
            normalMapAssetPath = Path.Combine(directory, normalMapName_c);
            normalMapTexture = AssetDatabase.LoadAssetAtPath(normalMapAssetPath, typeof(Texture2D)) as Texture2D;
        }

        return normalMapTexture;
    }


    private static Material CreateMatFromTx(Texture2D baseTexture, Texture2D normalMapTexture, Shader shader)
    {
        var path = AssetDatabase.GetAssetPath(baseTexture);
        if (File.Exists(path))
        {
            path = Path.GetDirectoryName(path);
        }

        var mat = new Material(shader) { mainTexture = baseTexture };
        mat.SetFloat("_Smoothness", 0.2f);

        if (normalMapTexture != null)
        {
            mat.SetTexture("_BumpMap", normalMapTexture);
            mat.SetFloat("_BumpScale", 2.0f);
        }

        AssetDatabase.CreateAsset(mat, Path.Combine(path, string.Format("{0}.mat", baseTexture.name)));

        return mat;
    }
}

