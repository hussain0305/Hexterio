using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private BaseTile Obner;
    private float TimePassed;
    private void Start()
    {
        Obner = gameObject.GetComponent<BaseTile>();
        TimePassed = 0;
    }
}
