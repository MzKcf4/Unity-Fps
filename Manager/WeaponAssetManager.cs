using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAssetManager : MonoBehaviour
{
    public static WeaponAssetManager Instance;
    
    public GameObject ak47ViewPrefab;
    public GameObject ak47WeaponPrefab;
    
    public GameObject r8ViewPrefab;
    public GameObject r8WeaponPrefab;
    
    public GameObject sawoffViewPrefab;
    public GameObject sawoffWeaponPrefab;
    
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
