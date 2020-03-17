using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlippingHexes : MonoBehaviour
{
    public GameObject Background_Right;
    public GameObject Background_Credits;
    public GameObject Background_Restart;
    public GameObject Right;
    public GameObject Credits;
    public GameObject Restart;

    public GameObject FailedText;
    public GameObject Reason;
    public GameObject CreditsBody;

    private Color OriginalRight;
    private Color OriginalCredits;
    private Color OriginalRestart;

    public void Start()
    {
        OriginalRight = Background_Right.GetComponent<Image>().color;
        OriginalCredits = Background_Credits.GetComponent<Image>().color;
        OriginalRestart = Background_Restart.GetComponent<Image>().color;
    }

    private void OnEnable()
    {
        this.transform.SetAsLastSibling();
    }

    public void TurnScreenOn(bool FRestart)
    {
        StartCoroutine(FlipHexes(FRestart));
    }
    
    public void TurnScreenOff(bool FRestart)
    {
        //StopAllCoroutines();
        StartCoroutine(ReverseHexes(FRestart));
    }

    public void CreditsToggle()
    {
        if (CreditsBody.activeSelf)
        {
            TurnScreenOff(false);
            CreditsBody.SetActive(false);
        }
        else
        {
            TurnScreenOn(false);
            StartCoroutine(DelayedActivation(false));
        }
    }

    IEnumerator FlipHexes(bool FRestart)
    {
        Background_Right.SetActive(false);
        Background_Restart.SetActive(false);
        Background_Credits.SetActive(false);

        foreach (Transform CurrentColumn in Right.transform)
        {
            foreach(Transform CurrentTile in CurrentColumn)
            {
                CurrentTile.gameObject.SetActive(true);
                CurrentTile.GetComponent<Animator>().SetTrigger("Flip");
            }
            yield return new WaitForSeconds(0.01f);
        }

        StartCoroutine(DelayedBackground(Background_Right, true));
        //Background_Right.SetActive(true);

        if (FRestart)
        {
            FailedText.SetActive(true);
            Reason.SetActive(true);

            foreach (Transform CurrentColumn in Restart.transform)
            {
                foreach (Transform CurrentTile in CurrentColumn)
                {
                    CurrentTile.gameObject.SetActive(true);
                    CurrentTile.GetComponent<Animator>().SetTrigger("Flip");
                }
                yield return new WaitForSeconds(0.01f);
            }

            StartCoroutine(DelayedBackground(Background_Restart, true));
            //Background_Restart.SetActive(true);
        }
        else
        {
            foreach (Transform CurrentColumn in Credits.transform)
            {
                foreach (Transform CurrentTile in CurrentColumn)
                {
                    CurrentTile.gameObject.SetActive(true);
                    CurrentTile.GetComponent<Animator>().SetTrigger("Flip");
                }
                yield return new WaitForSeconds(0.0001f);
            }

            StartCoroutine(DelayedBackground(Background_Credits, true));
            //Background_Credits.SetActive(true);
        }
    }

    IEnumerator ReverseHexes(bool FRestart)
    {
        if (FRestart)
        {
            FailedText.SetActive(false);
            Reason.SetActive(false);
            Background_Restart.SetActive(false);

        }

        else
        {
            for(int loop = (Credits.transform.childCount - 1); loop >= 0; loop--)
            {
                foreach (Transform CurrentTile in Credits.transform.GetChild(loop))
                {
                    CurrentTile.GetComponent<Animator>().SetTrigger("Reset");
                }
                yield return new WaitForSeconds(0.0001f);
            }

            for (int loop = (Right.transform.childCount - 1); loop >= 0; loop--)
            {
                foreach (Transform CurrentTile in Right.transform.GetChild(loop))
                {
                    CurrentTile.GetComponent<Animator>().SetTrigger("Reset");
                }
                yield return new WaitForSeconds(0.0001f);
            }

            Background_Credits.SetActive(false);
        }
        
        Background_Right.SetActive(false);

        if (!FRestart)
        {
            yield return new WaitForSeconds(0.6f);
        }
        
        DisableMe();
    }

    IEnumerator DelayedActivation(bool FRestart)
    {
        yield return new WaitForSeconds(0.25f);
        if (FRestart)
        {
            FailedText.SetActive(true);
            Reason.SetActive(true);

        }
        else
        {
            CreditsBody.SetActive(true);
        }
    }

    IEnumerator DelayedBackground(GameObject BackgroundInFocus, bool Value)
    {
        yield return new WaitForSeconds(0.25f);
        BackgroundInFocus.SetActive(Value);
    }

    public void DisableMe()
    {
        StopAllCoroutines();
        this.gameObject.SetActive(false);
    }
}

