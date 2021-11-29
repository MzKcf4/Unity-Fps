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
    public TextMeshProUGUI inputFieldAddress;
    
    // Start is called before the first frame update
    void Start()
    {
        if (NetworkManager.singleton.networkAddress != "localhost") { inputFieldAddress.text = NetworkManager.singleton.networkAddress; }
        
        btnHost.onClick.AddListener(HostServer);
        btnConnect.onClick.AddListener(ConnectToServer);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void HostServer()
    {
        NetworkManager.singleton.StartHost();
    }
    
    public void ConnectToServer()
    {
        NetworkManager.singleton.networkAddress = inputFieldAddress.text;
        NetworkManager.singleton.StartClient();
    }
}
