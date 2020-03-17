using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMode_AllTiles : BaseGameMode
{
    private int TilesTraversed;
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
    }


    override public void TileWasSteppedOn(BaseTile WhichTile){
        if(WhichTile.GetFlipCount() == 0)
        {
            TilesTraversed++;
        }
    }

    override public void ResetLocalLevelAtributes()
    {
        TilesTraversed = 0;
    }

    public override bool HasConditionBeenMet()
    {
        if (TilesTraversed >= NoOfTilesInLevel)
        {
            return true;
        }
        return false;
    }

    override public void TileWasSteppedOff(BaseTile WhichTile){ }

    override public void TileWasDestroyed(BaseTile WhichTile){ }
}
