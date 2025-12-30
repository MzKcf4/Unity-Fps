using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;


public class WeaponAnimationExtractor
{
    /*
    [MenuItem("Assets/QC/0. Extract animations (Click on animation FBX file)")]
    public static void ExtractAnimations()
    {
        GameObject selectedObject = Selection.activeObject;
        string ext = Path.GetFileName(AssetDatabase.GetAssetPath(selectedObject));
        if (!ext.Contains(".fbx"))
        {
            Debug.LogError("Only .fbx file is supported");
            return;
        }

        var fbxPath = AssetDatabase.GetAssetPath(selectedObject);



        //Find the names of embedded materials in the fbx
        IEnumerable<Object> embeddedAnimationClips = AssetDatabase.LoadAllAssetsAtPath(fbxPath)
            .Where(x => x.GetType() == typeof(AnimationClip));

        foreach (var obj in embeddedAnimationClips)
        {
            var animationClip = obj as AnimationClip;
            GameObject prefab = AnimationUtility. .GetPrefabObject(animationClip);
            AssetDatabase.cop
            Debug.Log(obj.name + ";" + obj.GetType());
        }
    }
    */
}