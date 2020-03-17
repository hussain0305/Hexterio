using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LandingPageManager : MonoBehaviour
{
    public GameObject CompanyLogoMain;

    private void Awake()
    {
        float w = (Screen.width / 2560.0f);
        float h = (Screen.height / 1440.0f);
        CompanyLogoMain.gameObject.transform.localScale = new Vector3(w, h, 1);
    }
    public void Start()
    {
        float w = (Screen.width / 2560.0f);
        float h = (Screen.height / 1440.0f);
        CompanyLogoMain.gameObject.transform.localScale = new Vector3(w, h, 1);
        StartCoroutine(DestroyLogo());
        StartCoroutine(Proceed());
    }

    IEnumerator Proceed()
    {
        yield return new WaitForSeconds(5);
        SceneManager.LoadScene(1);
    }

    IEnumerator DestroyLogo()
    {
        yield return new WaitForSeconds(3.25f);
        CompanyLogoMain.transform.parent.gameObject.SetActive(false);
    }
}
