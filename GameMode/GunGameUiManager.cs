using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GunGameUiManager : MonoBehaviour
{
    public static GunGameUiManager Instance;

    [SerializeField] private TextMeshProUGUI infoTextBlueTeam;
    [SerializeField] private TextMeshProUGUI infoTextRedTeam;
    [SerializeField] private TextMeshProUGUI targetLevelText;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateScore(TeamEnum team , int currLevel, int maxLevel , int killsPerLevel , int currKills)
    {
        string levelText = currLevel.ToString();

        string killsText = "";
        for (int i = 0; i < currKills; i++)
            killsText += "|";

        string remainingText = "";
        for (int i = 0; i < killsPerLevel - currKills; i++)
            remainingText += "-";


        string scoreText;
        if (TeamEnum.Blue == team)
            scoreText = killsText + remainingText;
        else
            scoreText = remainingText + killsText;

        string fullText;
        if (TeamEnum.Blue == team)
            fullText = scoreText + " " + levelText;
        else
            fullText = levelText + " " + scoreText;


        if (team == TeamEnum.Blue)
            infoTextBlueTeam.text = fullText;
        else
            infoTextRedTeam.text = fullText;
    }

    public void SetWin(TeamEnum team)
    {
        if (team == TeamEnum.Blue)
        {
            infoTextRedTeam.SetText("");
            infoTextBlueTeam.SetText("Blue team wins");
        }
        else
        {
            infoTextRedTeam.SetText("Red team wins");
            infoTextBlueTeam.SetText("");
        }
    }

    public void SetTargetLevel(int level)
    {
        targetLevelText.SetText(level.ToString());
    }
}
