using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMode_DestroyAll : BaseGameMode
{
    private int TilesDestroyed;
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
    }


    override public void TileWasSteppedOn(BaseTile WhichTile) { }

    override public void ResetLocalLevelAtributes()
    {
        TilesDestroyed = 0;
    }

    public override bool HasConditionBeenMet()
    {
        if (TilesDestroyed >= NoOfTilesInLevel - 1)
        {
            return true;
        }
        return false;
    }

    override public void TileWasSteppedOff(BaseTile WhichTile) { }

    override public void TileWasDestroyed(BaseTile WhichTile) {

        TilesDestroyed++;

    }
}
