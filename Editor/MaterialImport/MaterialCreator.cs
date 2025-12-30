using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using System.IO;

public class MaterialCreator
{
    public static void CreateMaterial(string textureFolderAssetPath, string materialFolderAssetPath, MaterialSettings materialSettings)
    {
        Debug.Log("Mapping textures to material file : " + materialSettings.MaterialName);

        string baseTexturePath = Path.Combine(textureFolderAssetPath, materialSettings.baseMapName + ".png");
        Texture2D baseTexture = AssetDatabase.LoadAssetAtPath(baseTexturePath,typeof(Texture2D)) as Texture2D;

        string normalMapTexturePath = Path.Combine(textureFolderAssetPath, materialSettings.normalMapName + ".png");
        Texture2D normalMapTexture = string.IsNullOrEmpty(materialSettings.normalMapName) 
                                    ? null 
                                    : AssetDatabase.LoadAssetAtPath(normalMapTexturePath,typeof(Texture2D)) as Texture2D;

        Debug.Log(baseTexturePath);
        Debug.Log(normalMapTexture);

        FixTexture(baseTexture, false);
        FixTexture(normalMapTexture, true);

        Shader litShader = Shader.Find("Universal Render Pipeline/Lit");

        Material material = CreateMaterial(baseTexture , normalMapTexture , litShader , materialSettings);
        AssetDatabase.CreateAsset(material, Path.Combine(materialFolderAssetPath, string.Format("{0}.mat", materialSettings.MaterialName)));
        Debug.Log("Material created : " + materialSettings.MaterialName);
    }

    private static void FixTexture(Texture2D texture , bool isNormalMap)
    {
        if(texture == null) return;
        //if (!isNormalMap)
        //    return;

        string path = AssetDatabase.GetAssetPath(texture);
        TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
        // textureImporter.sRGBTexture = false;        

        if (isNormalMap)
        {
            textureImporter.mipmapEnabled = false;
            textureImporter.textureType = TextureImporterType.NormalMap;
        }
        
        else
        {
            textureImporter.mipmapEnabled = true;
            textureImporter.textureType = TextureImporterType.Default;
        }
         
        AssetDatabase.ImportAsset(path);
    }

    private static Material CreateMaterial(Texture2D baseTexture, Texture2D normalMapTexture, Shader shader, MaterialSettings materialSetting)
    {
        var mat = new Material(shader) { mainTexture = baseTexture };
        mat.SetFloat("_Metallic", 0.0f);
        mat.SetFloat("_Smoothness", 0.2f);

        if (normalMapTexture != null)
        {
            mat.SetTexture("_BumpMap", normalMapTexture);
            mat.SetFloat("_BumpScale", 0.8f);
            mat.SetFloat("_Metallic", 0.2f);
            mat.SetFloat("_Smoothness", 0.5f);
        }

        if (materialSetting.isTransparent)
        {
            mat.SetFloat("_Mode", 3); // 3 = Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        }

        /*
         string path = AssetDatabase.GetAssetPath(texture);
         TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
         textureImporter.textureType = newType;
         AssetDatabase.ImportAsset(path);
        */

        return mat;
    }
}

