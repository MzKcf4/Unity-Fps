using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;

public class FpsUiOptionMenu : MonoBehaviour
{
    public GameObject optionPanel;
    public ButtonManagerBasic applyButton;
    public ButtonManagerBasic resetButton;
    
    public SliderManager audioSlider;
    
    void Start()
    {
        applyButton.clickEvent.AddListener( () => PlayerContext.Instance.SavePlayerSettings());
        resetButton.clickEvent.AddListener( () => {
            PlayerContext.Instance.LoadPlayerSettings();
            audioSlider.UpdateUI();
        });
        audioSlider.sliderEvent.AddListener( newVol => PlayerContext.Instance.OnAudioVolumeChanged(newVol));
        
        PlayerContext.Instance.onOptionMenuToggleEvent.AddListener(ToggleOptionPanel);
        
        LoadFromPlayerContext();
    }
    
    public void ToggleOptionPanel()
    {
        optionPanel.SetActive(!optionPanel.active);
    }
    
    public void LoadFromPlayerContext()
    {
        audioSlider.mainSlider.SetValueWithoutNotify(PlayerContext.Instance.playerSettingDto.audioMasterVolume);
        audioSlider.UpdateUI();
    }
}
