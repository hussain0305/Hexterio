using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseGameMode : MonoBehaviour
{
    protected int NoOfTilesInLevel;
    protected BaseTile LevelStartTile;
    protected BaseTile LevelEndTile;

    public WorldManager Manager;
    public MenuManager MenuMan;
    public PlayerController Player;
    
    // Start is called before the first frame update
    public void Start()
    {
        if (!Player)
        {
            Player = GameObject.FindGameObjectWithTag("Scoreboard").GetComponent<PlayerController>();
        }
        if (!Manager)
        {
            Manager = GameObject.FindObjectOfType<WorldManager>();
        }
        if (!MenuMan)
        {
            MenuMan = GameObject.FindObjectOfType<MenuManager>();
        }

        Player.SetScoreboardState(false);
    }

    public void NewLevelStarted(GameObject LevelHead)
    {
        NoOfTilesInLevel = LevelHead.transform.childCount - 1;
        LevelStartTile = LevelHead.GetComponent<LevelManager>().GetLevelStart();
        LevelEndTile = LevelHead.GetComponent<LevelManager>().GetLevelEnd();
        ResetLocalLevelAtributes();
    }

    public void LevelEnded()
    {
        ResetLocalLevelAtributes();
        Manager.EndThisLevel();
    }

    public virtual void ResetLocalLevelAtributes() {}

    public virtual void TileWasSteppedOn(BaseTile WhichTile) {}

    public virtual void TileWasSteppedOff(BaseTile WhichTile) {}

    public virtual void TileWasDestroyed(BaseTile WhichTile) {}

    public virtual bool HasConditionBeenMet() { return false; }
}
