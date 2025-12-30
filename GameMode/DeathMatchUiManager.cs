using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DeathMatchUiManager : MonoBehaviour
{
    public static DeathMatchUiManager Instance;

    [SerializeField] private TextMeshProUGUI uiTextScoreBlue;
    [SerializeField] private TextMeshProUGUI uiTextScoreRed;
    [SerializeField] private TextMeshProUGUI uiTextScoreTarget;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateScores(int blueScore, int redScore)
    {
        uiTextScoreBlue.SetText(blueScore.ToString());
        uiTextScoreRed.SetText(redScore.ToString());
    }

    public void SetTargetScore(int level)
    {
        uiTextScoreTarget.SetText(level.ToString());
    }


    public void SetWin(TeamEnum team)
    {
        if (team == TeamEnum.Blue)
        {
            uiTextScoreBlue.SetText("Blue team wins");
        }
        else
        {
            uiTextScoreRed.SetText("Red team wins");
        }
    }


}
