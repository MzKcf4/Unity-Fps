using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;

public class MainMenuNetworkHUD : MonoBehaviour
{
    public Button btnHost;
    public Button btnConnect;
    public TMP_InputField inputFieldAddress;
    public TMP_InputField inputFieldPlayerName;
    
    // UpdatePlayerName
    
    // Start is called before the first frame update
    void Start()
    {
        if (string.IsNullOrEmpty(NetworkManager.singleton.networkAddress))
            inputFieldAddress.text = "localhost";
        else
            inputFieldAddress.text = NetworkManager.singleton.networkAddress;
        
        if( LocalPlayerContext.Instance && !string.IsNullOrEmpty(LocalPlayerContext.Instance.playerSettingDto.playerName))
            inputFieldPlayerName.text = LocalPlayerContext.Instance.playerSettingDto.playerName;
        
        btnHost.onClick.AddListener(HostServer);
        btnConnect.onClick.AddListener(ConnectToServer);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void HostServer()
    {
        LocalPlayerContext.Instance.UpdatePlayerName(inputFieldPlayerName.text);
        
        NetworkManager.singleton.StartHost();
    }
    
    public void ConnectToServer()
    {
        LocalPlayerContext.Instance.UpdatePlayerName(inputFieldPlayerName.text);
        
        NetworkManager.singleton.StartClient();
        NetworkManager.singleton.networkAddress = inputFieldAddress.text;
    }
}
