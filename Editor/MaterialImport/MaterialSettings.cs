using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MaterialSettings
{
    // The name should match the material name in weapon FBX
    public string MaterialName;

    // The texture names
    public string baseMapName;
    public string normalMapName;
    public string specMapName;
    public bool isTransparent;

    public override string ToString()
    {
        return MaterialName + " = "  + " baseMap : " + baseMapName + " ; Normal : " + normalMapName + " ; Spec : " + specMapName;
    }

}

