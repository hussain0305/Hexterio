using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueScript : MonoBehaviour
{
    private WorldManager Manager;
    private BaseTile Owner;

    public void Start()
    {
        Manager = GameObject.FindObjectOfType<WorldManager>();
    }

    public void SetOwner(BaseTile Own)
    {
        Owner = Own;
    }
    public void NextValue()
    {
        if (!Manager)
        {
            Manager = GameObject.FindObjectOfType<WorldManager>();
        }
        Owner.DisplayNextTileValue();
    }
}
