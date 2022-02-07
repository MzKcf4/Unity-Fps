using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using UnityEngine.UI;

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

    [SerializeField] public Toggle crosshairLerpToggle;
    [SerializeField] public HorizontalSelector crosshairSizeSelector;

    void Start()
    {
        LocalPlayerSettingManager.Instance.OnPlayerSettingUpdateEvent.AddListener(LoadFromPlayerContext);

        applyButton.clickEvent.AddListener(SaveValuesToPlayerSetting);
        resetButton.clickEvent.AddListener( () => {
            LocalPlayerSettingManager.Instance.LoadPlayerSettings(true);
        });
        
        // ------- Audio ------- //
        audioMenuButton.clickEvent.AddListener( () => SwitchToAudioPanel());
 
        // ------ Mouse -------// 
        mouseMenuButton.clickEvent.AddListener( () => SwitchToMousePanel());

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
        PlayerSettingDto playerSettingDto = LocalPlayerSettingManager.Instance.GetLocalPlayerSettings();
        
        audioSlider.mainSlider.SetValueWithoutNotify(playerSettingDto.audioMasterVolume);
        audioSlider.UpdateUI();
        
        mouseSpeedSlider.mainSlider.SetValueWithoutNotify(playerSettingDto.mouseSpeed);
        mouseSpeedSlider.UpdateUI();
        
        mouseSpeedScopedSlider.mainSlider.SetValueWithoutNotify(playerSettingDto.mouseSpeedZoomed);
        mouseSpeedScopedSlider.UpdateUI();

        crosshairLerpToggle.isOn = playerSettingDto.isLerpCrosshair;

        if (playerSettingDto.crosshairSize == CrosshairSizeEnum.Small)
            crosshairSizeSelector.index = 0;
        else
            crosshairSizeSelector.index = 1;
        crosshairSizeSelector.UpdateUI();
    }

    private void SaveValuesToPlayerSetting()
    {
        PlayerSettingDto playerSettingDto = LocalPlayerSettingManager.Instance.GetLocalPlayerSettings();
        playerSettingDto.audioMasterVolume = audioSlider.mainSlider.value;
        playerSettingDto.mouseSpeed = mouseSpeedSlider.mainSlider.value;
        playerSettingDto.mouseSpeedZoomed = mouseSpeedScopedSlider.mainSlider.value;
        playerSettingDto.isLerpCrosshair = crosshairLerpToggle.isOn;
        playerSettingDto.crosshairSize = crosshairSizeSelector.index == 0 ? CrosshairSizeEnum.Small : CrosshairSizeEnum.Standard;

        LocalPlayerSettingManager.Instance.SavePlayerSettings();
    }
}
