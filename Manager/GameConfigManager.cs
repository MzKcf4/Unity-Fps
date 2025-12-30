using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConfigManager : MonoBehaviour
{
    public static GameConfigManager Instance { get; private set; }
    Dictionary<string , string> keyValuePairs = new Dictionary<string , string>();

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
