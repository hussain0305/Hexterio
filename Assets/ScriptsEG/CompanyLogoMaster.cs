using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanyLogoMaster : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        this.gameObject.transform.localScale = new Vector3(Screen.width / 2560, Screen.height / 1440, 1);
    }

    void Start()
    {
        this.gameObject.transform.localScale = new Vector3(Screen.width / 2560, Screen.height / 1440, 1);
    }

}
