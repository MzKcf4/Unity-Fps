using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Animancer;

[CreateAssetMenu(menuName = "FpsResource/CharacterResources")]
public class CharacterResources : SerializedScriptableObject
{
    public List<AudioClip> hurtVoiceList = new List<AudioClip>();
    public List<AudioClip> deathVoiceList = new List<AudioClip>();

    public GameObject projectilePrefab;
}

