using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    public static WaypointManager Instance;
    public List<Transform> mapGoalList = new List<Transform>();
    
    void Awake()
    {
        Instance = this;
    }
}
