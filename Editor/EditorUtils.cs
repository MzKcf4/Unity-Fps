using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FpsEditorUtils
{
    public static string ConvertToAssetPath(string fileSystemPath)
    {
        if (fileSystemPath.StartsWith(Application.dataPath))
        {
            return "Assets" + fileSystemPath.Substring(Application.dataPath.Length);
        }
        else
        {
            throw new Exception("Cannot conver to assetPath : " + fileSystemPath);
        }
    }
}

