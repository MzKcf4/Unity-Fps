using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalContext : MonoBehaviour
{
    public static LocalContext Instance;
    
    public Canvas dynamicCanvas;
    
    void Awake()
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
