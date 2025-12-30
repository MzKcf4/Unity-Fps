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

public class ImageBlueColorConvertor
{
    [MenuItem("Assets/GoldSrc/Convert Blue To Transparent")]
    public static void ConvertBlueToTransparent()
    {
        var selectedObjects = Selection.objects;
        foreach (var selectedObject in selectedObjects)
        {
            Debug.Log("Converting: " + AssetDatabase.GetAssetPath(selectedObject));
            DoConvert(selectedObject);
        }

        AssetDatabase.Refresh();
    }

    private static void DoConvert(UnityEngine.Object selectedObject)
    {
        string ext = Path.GetFileName(AssetDatabase.GetAssetPath(selectedObject));
        if (!ext.Contains(".bmp"))
        {
            Debug.LogError("Only .bmp file is supported");
            return;
        }

        var fullPath = Path.Combine(Application.dataPath, AssetDatabase.GetAssetPath(selectedObject)).Replace("Assets\\", "");

        var folderPath = Path.GetDirectoryName(fullPath);
        var fileName = Path.GetFileName(fullPath);
        var pngFileName = Path.GetFileNameWithoutExtension(fileName) + ".png";
        var newFullPath = Path.Combine(folderPath, pngFileName);

        Bitmap bmp = new Bitmap(fullPath);
        for (int x = 0; x < bmp.Width; x++)
        {
            for (int y = 0; y < bmp.Height; y++)
            {
                System.Drawing.Color gotColor = bmp.GetPixel(x, y);
                if (gotColor.R == 0 && gotColor.G == 0 && gotColor.B == 255)
                {
                    bmp.SetPixel(x, y, System.Drawing.Color.FromArgb(0, gotColor.R, gotColor.G, gotColor.B));
                    continue;
                }
            }
        }
        bmp.Save(newFullPath, ImageFormat.Png);
        bmp.Dispose();
    }
}

