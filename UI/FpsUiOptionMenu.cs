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
        applyButton.clickEvent.AddListener( () => LocalPlayerContext.Instance.SavePlayerSettings());
        resetButton.clickEvent.AddListener( () => {
            LocalPlayerContext.Instance.LoadPlayerSettings();
            audioSlider.UpdateUI();
        });
        
        // ------- Audio ------- //
        audioMenuButton.clickEvent.AddListener( () => SwitchToAudioPanel());
        audioSlider.sliderEvent.AddListener( newVol => LocalPlayerContext.Instance.OnAudioVolumeChanged(newVol));
        
        
        // ------ Mouse -------// 
        mouseMenuButton.clickEvent.AddListener( () => SwitchToMousePanel());
        mouseSpeedSlider.sliderEvent.AddListener(newSpeed => LocalPlayerContext.Instance.OnMouseSpeedChanged(newSpeed));
        mouseSpeedScopedSlider.sliderEvent.AddListener(newSpeed => LocalPlayerContext.Instance.OnMouseSpeedZoomedChanged(newSpeed));
        
        LocalPlayerContext.Instance.onOptionMenuToggleEvent.AddListener(ToggleOptionPanel);
        
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
        bool newActiveState = !optionPanel.activeSelf;
        optionPanel.SetActive(newActiveState);
        // reverse of optionalPanel activeState.
        LocalPlayerContext.Instance.TogglePlayerControl(!newActiveState);
    }
    
    public void LoadFromPlayerContext()
    {
        PlayerSettingDto playerSettingDto = LocalPlayerContext.Instance.playerSettingDto;
        
        audioSlider.mainSlider.SetValueWithoutNotify(playerSettingDto.audioMasterVolume);
        audioSlider.UpdateUI();
        
        mouseSpeedSlider.mainSlider.SetValueWithoutNotify(playerSettingDto.mouseSpeed);
        mouseSpeedSlider.UpdateUI();
        
        mouseSpeedScopedSlider.mainSlider.SetValueWithoutNotify(playerSettingDto.mouseSpeedZoomed);
        mouseSpeedScopedSlider.UpdateUI();
    }
}
