using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class LevelManager : MonoBehaviour
{
    public bool HasLevelCompletionAchievement = false;
    public string AchievementID;
    public BaseTile LevelStart;
    public BaseTile LevelEnd;

    public BaseTile GetLevelStart()
    {
        return LevelStart;
    }

    public BaseTile GetLevelEnd()
    {
        return LevelEnd;
    }

    public void LevelAchievementCheck()
    {
        if (HasLevelCompletionAchievement)
        {
            SteamUserStats.SetAchievement(AchievementID);
            SteamUserStats.StoreStats();
        }
    }
}
