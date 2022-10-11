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
        Texture2D baseTexture = AssetDatabase.LoadAssetAtPath(
                                   Path.Combine(textureFolderAssetPath, materialSettings.baseMapName + ".png"),
                                   typeof(Texture2D)) as Texture2D;
        

        Texture2D normalMapTexture = string.IsNullOrEmpty(materialSettings.normalMapName) 
                                    ? null 
                                    : AssetDatabase.LoadAssetAtPath(
                                        Path.Combine(textureFolderAssetPath, materialSettings.normalMapName + ".png") ,
                                        typeof(Texture2D)) as Texture2D;

        FixTexture(baseTexture, false);
        if (normalMapTexture != null)
            FixTexture(normalMapTexture, true);

        Shader litShader = Shader.Find("Universal Render Pipeline/Lit");

        Material material = CreateMaterial(baseTexture , normalMapTexture , litShader);
        AssetDatabase.CreateAsset(material, Path.Combine(materialFolderAssetPath, string.Format("{0}.mat", materialSettings.MaterialName)));
        Debug.Log("Material created : " + materialSettings.MaterialName);
    }

    private static void FixTexture(Texture2D texture , bool isNormalMap)
    {
        if (!isNormalMap)
            return;

        string path = AssetDatabase.GetAssetPath(texture);
        TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
        // textureImporter.sRGBTexture = false;        

        //if(isNormalMap)
        //{
            textureImporter.mipmapEnabled = false;
            textureImporter.textureType = TextureImporterType.NormalMap;
        //}
            
        AssetDatabase.ImportAsset(path);
    }

    private static Material CreateMaterial(Texture2D baseTexture, Texture2D normalMapTexture, Shader shader)
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

        /*
         string path = AssetDatabase.GetAssetPath(texture);
         TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
         textureImporter.textureType = newType;
         AssetDatabase.ImportAsset(path);
        */

        return mat;
    }
}

