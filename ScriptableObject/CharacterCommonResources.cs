using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FpsRes/CharacterCommonResources")]
public class CharacterCommonResources : ScriptableObject
{
    public List<AudioClip> hurtSoundList;
    public List<AudioClip> hurtHeadShotSoundList;
    public List<AudioClip> deathSoundList;
    public List<AudioClip> footstepList;
}
