using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMode_Sum : BaseGameMode
{
    public Image ScoreboardIndicator;
    private int Sum;
    private int Target;
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        StartCoroutine(DelayedFetch());
    }


    override public void TileWasSteppedOn(BaseTile WhichTile) {
        if (WhichTile.GetTileType() == TileType.LevelEnd)
        {
            return;
        }
        Sum += WhichTile.GetTileValue();
        if (Sum == Target)
        {
            ScoreboardIndicator.color = Color.green;
        }
        else
        {
            ScoreboardIndicator.color = Color.red;
        }
        Player.AddToScore(WhichTile.GetTileValue());
    }

    override public void ResetLocalLevelAtributes()
    {
        Sum = 0;
        Player.ResetScores();
        ScoreboardIndicator.color = Color.red;

        if (LevelEndTile)
        {
            Target = LevelEndTile.GetFirstValue();
        }
    }

    public override bool HasConditionBeenMet()
    {
        if (Sum == Target)
        {
            return true;
        }
        return false;
    }

    override public void TileWasSteppedOff(BaseTile WhichTile) { }

    override public void TileWasDestroyed(BaseTile WhichTile)
    {

    }

    IEnumerator DelayedFetch()
    {
        yield return new WaitForSeconds(0.5f);
        Player.SetScoreboardState(true);
    }
}
