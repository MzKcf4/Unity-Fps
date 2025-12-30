using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using Michsky.UI.Shift;

public class MainMenuNetworkHUD : MonoBehaviour
{
    public Button btnHost;
    public Button btnConnect;
    public TMP_InputField inputFieldAddress;
    public TMP_InputField inputFieldPlayerName;
    
    // Start is called before the first frame update
    void Start()
    {
        if (string.IsNullOrEmpty(NetworkManager.singleton.networkAddress))
            inputFieldAddress.text = "localhost";
        else
            inputFieldAddress.text = NetworkManager.singleton.networkAddress;

        if (LocalPlayerSettingManager.Instance && !string.IsNullOrEmpty(LocalPlayerSettingManager.Instance.GetPlayerName()))
            inputFieldPlayerName.text = LocalPlayerSettingManager.Instance.GetPlayerName();
        else 
            inputFieldPlayerName.text = "player";

        btnHost.onClick.AddListener(DoHostServer);
        btnConnect.onClick.AddListener(DoConnectToServer);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void DoHostServer()
    {
        LocalPlayerSettingManager.Instance.UpdatePlayerName(inputFieldPlayerName.text);
        
        NetworkManager.singleton.StartHost();
    }
    
    public void DoConnectToServer()
    {
        LocalPlayerSettingManager.Instance.UpdatePlayerName(inputFieldPlayerName.text);
        
        NetworkManager.singleton.networkAddress = inputFieldAddress.text;
        NetworkManager.singleton.StartClient();
    }
}
