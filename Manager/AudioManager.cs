using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    // List of audioSource that will be placed in targeted position and play one shot
    [SerializeField] private List<AudioSource> audioSourceList = new List<AudioSource>();
    [SerializeField] public AudioSource localPlayerAudioSource;
    
    [SerializeField] private AudioMixerGroup audioMixerMasterGroup;
    
    void Awake()
    {
        Instance = this;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        foreach(AudioSource audioSource in audioSourceList)
        {
            audioSource.outputAudioMixerGroup = audioMixerMasterGroup;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0.5f;
            audioSource.spread = 1f;
        }
        localPlayerAudioSource.outputAudioMixerGroup = audioMixerMasterGroup;
    }
    
    public void PlaySoundAtPosition(AudioClip audioClip , Vector3 position)
    {
        AudioSource audioSource = GetAvailableAudioSource();
        audioSource.transform.position = position;
        audioSource.PlayOneShot(audioClip);
    }
    
    private AudioSource GetAvailableAudioSource()
    {
        foreach(AudioSource audioSource in audioSourceList)
        {
            if(!audioSource.isPlaying)
                return audioSource;
        }
        
        Debug.LogWarning("Run out of available audio source !");
        return audioSourceList[0];
    }
}
