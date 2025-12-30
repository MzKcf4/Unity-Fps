using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class WeaponAnimationImportContext
{
    public string weaponName;
    // The path containing the .qc .fbx files
    public string modelFolderAssetPath;
    public string modelFolderFileSystemPath;

    public string qcFileSystemPath;
    public List<WeaponModelAttachment> weaponAttachmentList = new List<WeaponModelAttachment>();

    public bool isShotgunReloadFound = false;

    // The path of WeaponResource ScriptableObject
    public string weaponResourceAssetPath;
    public WeaponResources weaponResources;

    public string vModelAssetPath;
    public string wModelAssetPath;

    public Dictionary<String, QcSequenceEventInfo> dictAnimEvent = new Dictionary<string, QcSequenceEventInfo>();
}

