using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    public static WaypointManager Instance;
    [HideInInspector] public List<Transform> mapGoalList = new List<Transform>();
    
    void Awake()
    {
        Instance = this;
        GameObject[] waypointObjs = GameObject.FindGameObjectsWithTag(Constants.TAG_BOT_WAYPOINT);

        foreach (GameObject waypointObj in waypointObjs)
            mapGoalList.Add(waypointObj.transform);
    }
}
