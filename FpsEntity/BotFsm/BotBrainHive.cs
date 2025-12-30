
using Pathfinding;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BotBrainHive : MonoBehaviour
{
    public static BotBrainHive Instance { get; private set; }

    private readonly ActionCooldown calculateCooldown = new ActionCooldown { interval = 1.0f };

    private Dictionary<TeamEnum, HashSet<GraphNode>> dictTeamKilledNodes = new Dictionary<TeamEnum, HashSet<GraphNode>>();



    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        dictTeamKilledNodes.Add(TeamEnum.Red, new HashSet<GraphNode>());
        dictTeamKilledNodes.Add(TeamEnum.Blue, new HashSet<GraphNode>());
    }

    void Update()
    {
        if (!calculateCooldown.CanExecuteAfterDeltaTime(true))
            return;

        foreach (var item in dictTeamKilledNodes)
        {
            TeamEnum team = item.Key;
            HashSet<GraphNode> killedNodes = item.Value;

            if(killedNodes.Count == 0)
                continue;

            AstarPath.active.AddWorkItem(() => {

                foreach (var killedNode in killedNodes)
                {
                    ReduceNodePenalty(killedNode);

                    killedNode.GetConnections((otherNode =>
                    {
                        ReduceNodePenalty(otherNode);
                    }));
                }
            });

            killedNodes.RemoveWhere(n => n.Penalty <= 0);
        }
    }

    protected void ReduceNodePenalty(GraphNode node)
    {
        if (node.Penalty <= 100)
            node.Penalty = 0;
        else
            node.Penalty -= 100;
    }


    public void RegisterNodeKilled(TeamEnum team, Vector3 killedPosition)
    {
        AstarPath.active.AddWorkItem(() => {
            // Safe to update graphs here
            var node = AstarPath.active.GetNearest(killedPosition, NearestNodeConstraint.Walkable).node;
            node.Penalty += 3100;
            node.GetConnections((otherNode =>
            {
                otherNode.Penalty += 3100;
            }));
            dictTeamKilledNodes[team].Add(node);
            List<GraphNode> reachableNodes = PathUtilities.GetReachableNodes(node);
            // Or you could set a tag
            // node.Tag = 3;
            Debug.Log($"{killedPosition} : Registered killed node at {(Vector3)node.position} for team {team}");
        });

        // var astarNode = AstarPath.active.GetNearest(killedPosition).node;
    }
}