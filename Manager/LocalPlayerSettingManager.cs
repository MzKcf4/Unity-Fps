using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class LocalPlayerSettingManager : MonoBehaviour
{
    public static LocalPlayerSettingManager Instance;

    private PlayerSettingDto playerSettingDto;
    [HideInInspector] public UnityEvent OnPlayerSettingUpdateEvent = new UnityEvent();

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        LoadPlayerSettings(false);
    }

    public void LoadPlayerSettings(bool forceLoad)
    {
        if (!forceLoad && playerSettingDto != null)
            return;

        playerSettingDto = ES3.Load<PlayerSettingDto>(Constants.SETTING_KEY_LOCAL_PLAYER_SETTINGS, new PlayerSettingDto());
        ApplyPlayerSettings(playerSettingDto);
    }

    public void SavePlayerSettings()
    {
        ES3.Save(Constants.SETTING_KEY_LOCAL_PLAYER_SETTINGS, playerSettingDto);
        ApplyPlayerSettings(playerSettingDto);
    }

    public void ApplyPlayerSettings(PlayerSettingDto playerSettingDto)
    {
        this.playerSettingDto = playerSettingDto;
        LocalPlayerContext.Instance.audioMixerMaster.SetFloat("volume", playerSettingDto.audioMasterVolume);
        Crosshair.Instance.SetSize(playerSettingDto.crosshairSize);
        Crosshair.Instance.SetLerp(playerSettingDto.isLerpCrosshair);
        OnPlayerSettingUpdateEvent.Invoke();
    }

    public PlayerSettingDto GetLocalPlayerSettings()
    {
        LoadPlayerSettings(false);
        return playerSettingDto;
    }

    public void UpdatePlayerName(string newName)
    {
        playerSettingDto.playerName = newName;
        ES3.Save(Constants.SETTING_KEY_LOCAL_PLAYER_SETTINGS, playerSettingDto);
    }

    public string GetPlayerName()
    {
        LoadPlayerSettings(false);
        return playerSettingDto.playerName;
    }

    public float GetMouseSpeed()
    {
        return playerSettingDto.GetConvertedMouseSpeed();
    }

    public float GetMouseZoomedSpeed()
    {
        return playerSettingDto.GetConvertedMouseZoomedSpeed();
    }
}
