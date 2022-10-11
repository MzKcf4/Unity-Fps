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
    public List<WeaponModelAttachment> weaponAttachmentList = new List<WeaponModelAttachment>();

    public bool isShotgunReloadFound = false;

    // The path of WeaponResource ScriptableObject
    public string weaponResourceAssetPath;
    public WeaponResources weaponResources;

    public string vModelAssetPath;
    public string wModelAssetPath;


    // The standard animations available in WeaponResource
    public Dictionary<WeaponAnimType, QcSequenceEventInfo> dictStandardAnimTypeInfo = new Dictionary<WeaponAnimType, QcSequenceEventInfo>()
    {
        { WeaponAnimType.ANIM_IDLE , new QcSequenceEventInfo() },
        { WeaponAnimType.ANIM_FIRE , new QcSequenceEventInfo() },
        { WeaponAnimType.ANIM_RELOAD , new QcSequenceEventInfo() },
        { WeaponAnimType.ANIM_DRAW ,new QcSequenceEventInfo() },
        { WeaponAnimType.ANIM_RELOAD_PALLET_START , new QcSequenceEventInfo() },
        { WeaponAnimType.ANIM_RELOAD_PALLET_INSERT , new QcSequenceEventInfo() },
        { WeaponAnimType.ANIM_RELOAD_PALLET_END , new QcSequenceEventInfo() },
    };

    // Non standard sequenceEvents
    public List<QcSequenceEventInfo> nonStandardAnimEventInfoList = new List<QcSequenceEventInfo>();
}

