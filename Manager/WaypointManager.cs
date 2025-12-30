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
        mapGoalList = new List<Transform>();
        GameObject[] waypointObjs = GameObject.FindGameObjectsWithTag(Constants.TAG_TEAM_A_SPAWN);
        foreach (GameObject waypointObj in waypointObjs)
            mapGoalList.Add(waypointObj.transform);

        waypointObjs = GameObject.FindGameObjectsWithTag(Constants.TAG_TEAM_B_SPAWN);
        foreach (GameObject waypointObj in waypointObjs)
            mapGoalList.Add(waypointObj.transform);
    }
}
