using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class WeaponImportContext
{
    public string weaponName;
    // The path containing the .qc .fbx files
    public string modelFolderAssetPath;
    public string modelFolderFileSystemPath;

    public string qcFileSystemPath;
    // The path of WeaponResource ScriptableObject
    public string weaponResourceAssetPath;
    public WeaponResources weaponResources;

    public string vModelAssetPath;
    public string wModelAssetPath;


}

