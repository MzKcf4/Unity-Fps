using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

[System.Serializable]
public class CampingSpot
{
    public Vector3 Position;
    public Vector3 AverageLookDir;
    public int KillCount;
    public float LastUsedTime;
    public TeamEnum Team;
    public List<Vector3> LookDirections = new List<Vector3>();

    public CampingSpot(Vector3 pos, Vector3 lookDir, TeamEnum team)
    {
        Position = pos;
        AddLookDir(lookDir);
        KillCount = 1;
        LastUsedTime = Time.time;
        Team = team;
    }

    public void AddLookDir(Vector3 lookDir)
    {
        LookDirections.Add(lookDir);
        if (LookDirections.Count > 10)
            LookDirections.RemoveAt(0);

        // Recalculate average
        Vector3 sum = Vector3.zero;
        foreach (var dir in LookDirections)
            sum += dir;
        AverageLookDir = sum.normalized;
    }
}

public class CampingManager : NetworkBehaviour
{
    public static CampingManager Instance;

    public List<CampingSpot> campingSpots = new List<CampingSpot>();
    public float mergeDistance = 2.0f;

    void Awake()
    {
        Instance = this;
    }

    public void RegisterKill(Vector3 killerPos, Vector3 victimPos, TeamEnum killerTeam)
    {
        Vector3 lookDir = (victimPos - killerPos).normalized;

        CampingSpot existingSpot = campingSpots.FirstOrDefault(s => Vector3.Distance(s.Position, killerPos) < mergeDistance && s.Team == killerTeam);

        if (existingSpot != null)
        {
            // Update existing spot
            // Move position slightly towards new kill pos (weighted average)
            existingSpot.Position = Vector3.Lerp(existingSpot.Position, killerPos, 0.2f);
            existingSpot.AddLookDir(lookDir);
            existingSpot.KillCount++;
            existingSpot.LastUsedTime = Time.time;
        }
        else
        {
            // Create new spot
            campingSpots.Add(new CampingSpot(killerPos, lookDir, killerTeam));
        }

        // Cleanup old/weak spots periodically or if list gets too big
        if(campingSpots.Count > 50)
        {
            campingSpots = campingSpots.OrderByDescending(s => s.KillCount).Take(40).ToList();
        }
    }

    public void RegisterCampingResult(Vector3 spotPosition, bool successful)
    {
        CampingSpot existingSpot = campingSpots.FirstOrDefault(s => Vector3.Distance(s.Position, spotPosition) < mergeDistance);

        if (existingSpot != null)
        {
            if (successful)
            {
                existingSpot.KillCount++;
                existingSpot.LastUsedTime = Time.time;
            }
            else
            {
                existingSpot.KillCount--;
                if (existingSpot.KillCount <= 0)
                {
                    campingSpots.Remove(existingSpot);
                }
            }
        }
    }

    public CampingSpot GetBestCampingSpot(Vector3 botPos, TeamEnum botTeam)
    {
        if (campingSpots.Count == 0) return null;

        // Filter valid spots:
        // 1. Not too close to current position
        // 2. Not near enemy spawn (optional, maybe too complex for now, keep it simple)
        
        var validSpots = campingSpots.Where(s => Vector3.Distance(s.Position, botPos) > 5.0f && s.Team == botTeam).ToList();

        if (validSpots.Count == 0) return null;

        // Roulette Wheel Selection based on KillCount
        float totalScore = validSpots.Sum(s => s.KillCount);
        float randomValue = Random.Range(0, totalScore);
        float currentSum = 0;

        foreach (var spot in validSpots)
        {
            currentSum += spot.KillCount;
            if (currentSum >= randomValue)
                return spot;
        }

        return validSpots.Last();
    }
}
