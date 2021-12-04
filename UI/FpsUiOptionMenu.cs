using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;

public class FpsUiOptionMenu : MonoBehaviour
{
    public GameObject optionPanel;
    public GameObject audioContentPanel;
    public GameObject mouseContentPanel;
    
    public ButtonManagerBasic applyButton;
    public ButtonManagerBasic resetButton;
    
    public ButtonManagerBasic audioMenuButton;
    public ButtonManagerBasic mouseMenuButton;
    
    public SliderManager audioSlider;
    public SliderManager mouseSpeedSlider;
    public SliderManager mouseSpeedScopedSlider;
    
    void Start()
    {
        applyButton.clickEvent.AddListener( () => PlayerContext.Instance.SavePlayerSettings());
        resetButton.clickEvent.AddListener( () => {
            PlayerContext.Instance.LoadPlayerSettings();
            audioSlider.UpdateUI();
        });
        
        // ------- Audio ------- //
        audioMenuButton.clickEvent.AddListener( () => SwitchToAudioPanel());
        audioSlider.sliderEvent.AddListener( newVol => PlayerContext.Instance.OnAudioVolumeChanged(newVol));
        
        
        // ------ Mouse -------// 
        mouseMenuButton.clickEvent.AddListener( () => SwitchToMousePanel());
        mouseSpeedSlider.sliderEvent.AddListener(newSpeed => PlayerContext.Instance.OnMouseSpeedChanged(newSpeed));
        mouseSpeedScopedSlider.sliderEvent.AddListener(newSpeed => PlayerContext.Instance.OnMouseSpeedZoomedChanged(newSpeed));
        
        PlayerContext.Instance.onOptionMenuToggleEvent.AddListener(ToggleOptionPanel);
        
        LoadFromPlayerContext();
    }
    
    public void SwitchToMousePanel()
    {
        mouseContentPanel.SetActive(true);
        audioContentPanel.SetActive(false);
    }
    
    public void SwitchToAudioPanel()
    {
        mouseContentPanel.SetActive(false);
        audioContentPanel.SetActive(true);
    }
    
    
    public void ToggleOptionPanel()
    {
        optionPanel.SetActive(!optionPanel.active);
    }
    
    public void LoadFromPlayerContext()
    {
        PlayerSettingDto playerSettingDto = PlayerContext.Instance.playerSettingDto;
        
        audioSlider.mainSlider.SetValueWithoutNotify(playerSettingDto.audioMasterVolume);
        audioSlider.UpdateUI();
        
        mouseSpeedSlider.mainSlider.SetValueWithoutNotify(playerSettingDto.mouseSpeed);
        mouseSpeedSlider.UpdateUI();
        
        mouseSpeedScopedSlider.mainSlider.SetValueWithoutNotify(playerSettingDto.mouseSpeedZoomed);
        mouseSpeedScopedSlider.UpdateUI();
    }
}
