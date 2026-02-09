using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    public static WaypointManager Instance;
    public List<Transform> mapGoalList = new List<Transform>();

    public List<Transform> teamASpawnList = new List<Transform>();
    public List<Transform> teamBSpawnList = new List<Transform>();

    void Awake()
    {
        Instance = this;
        mapGoalList = new List<Transform>();
        teamASpawnList = new List<Transform>();
        teamBSpawnList = new List<Transform>();

        GameObject[] waypointObjs = GameObject.FindGameObjectsWithTag(Constants.TAG_TEAM_A_SPAWN);
        foreach (GameObject waypointObj in waypointObjs)
        {
            mapGoalList.Add(waypointObj.transform);
            teamASpawnList.Add(waypointObj.transform);
        }

        waypointObjs = GameObject.FindGameObjectsWithTag(Constants.TAG_TEAM_B_SPAWN);
        foreach (GameObject waypointObj in waypointObjs)
        {
            mapGoalList.Add(waypointObj.transform);
            teamBSpawnList.Add(waypointObj.transform);
        }
    }

    public List<Transform> GetSpawnPoints(TeamEnum team)
    {
        if (team == TeamEnum.Blue)
            return teamASpawnList;
        else if (team == TeamEnum.Red)
            return teamBSpawnList;
        else
            return mapGoalList;
    }
}
