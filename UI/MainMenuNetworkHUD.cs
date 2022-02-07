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

        if (LocalPlayerSettingManager.Instance && !string.IsNullOrEmpty(LocalPlayerSettingManager.Instance.GetPlayerName()))
            inputFieldPlayerName.text = LocalPlayerSettingManager.Instance.GetPlayerName();


        btnHost.onClick.AddListener(HostServer);
        btnConnect.onClick.AddListener(ConnectToServer);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void HostServer()
    {
        LocalPlayerSettingManager.Instance.UpdatePlayerName(inputFieldPlayerName.text);
        
        NetworkManager.singleton.StartHost();
    }
    
    public void ConnectToServer()
    {
        LocalPlayerSettingManager.Instance.UpdatePlayerName(inputFieldPlayerName.text);
        
        NetworkManager.singleton.StartClient();
        NetworkManager.singleton.networkAddress = inputFieldAddress.text;
    }
}
