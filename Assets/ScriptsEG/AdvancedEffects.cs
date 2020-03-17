using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdvancedEffects : MonoBehaviour
{
    public Text[] Texts;
    public Image[] Images;
    public GameObject UpperMarker;
    public GameObject LowerMarker;

    private bool MouseOnLeft;
    private bool LastMouseOnLeft;
    private bool CoroutineRunning;
    private Coroutine TheCoroutine;

    private List<Color> OriginalTexts;
    private List<Color> OriginalImages;

    void Start()
    {
        MouseOnLeft = true;
        LastMouseOnLeft = MouseOnLeft;
        CoroutineRunning = false;

        StoreColors();

        StartCoroutine(StartTracking());
    }

    public void StoreColors()
    {
        OriginalImages = new List<Color>();
        OriginalTexts = new List<Color>();

        foreach (Image CurrentImage in Images)
        {
            OriginalImages.Add(CurrentImage.color);
        }
        foreach(Text CurrentText in Texts)
        {
            OriginalTexts.Add(CurrentText.color);
        }
    }

    IEnumerator StartTracking()
    {
        yield return new WaitForSeconds(0.2f);

        while (true)
        {   
            if (Camera.main.ScreenToWorldPoint(Input.mousePosition).x < this.gameObject.transform.position.x && 
                ((Camera.main.ScreenToWorldPoint(Input.mousePosition).y > UpperMarker.transform.position.y) ||
                (Camera.main.ScreenToWorldPoint(Input.mousePosition).y < LowerMarker.transform.position.y)))
            {
                MouseOnLeft = true;
            }
            else
            {
                MouseOnLeft = false;
            }

            if (MouseOnLeft != LastMouseOnLeft)
            {
                if (CoroutineRunning)
                {
                    StopCoroutine(TheCoroutine);
                }
                if (MouseOnLeft)
                {
                    TheCoroutine = StartCoroutine(StartOpaquing());
                }
                else
                {
                    TheCoroutine = StartCoroutine(StartTransparenting());
                }
            }

            LastMouseOnLeft = MouseOnLeft;

            yield return new WaitForSeconds(0.25f);
        }
        
    }

    IEnumerator StartOpaquing()
    {
        CoroutineRunning = true;
        while (Images[0].color.a <= 0.98f)
        {
            int index = 0;
            foreach(Image CurrentImage in Images)
            {
                CurrentImage.color = Color.Lerp(CurrentImage.color, OriginalImages[index], 10 * Time.deltaTime);
                index++;
            }
            index = 0;
            foreach (Text CurrentText in Texts)
            {
                CurrentText.color = Color.Lerp(CurrentText.color, OriginalTexts[index], 10 * Time.deltaTime);
                index++;
            }
            yield return new WaitForEndOfFrame();
        }
        CoroutineRunning = false;
    }

    IEnumerator StartTransparenting()
    {
        CoroutineRunning = true;
        while (Images[0].color.a >= 0.1f)
        {
            int index = 0;
            foreach (Image CurrentImage in Images)
            {
                CurrentImage.color = Color.Lerp(CurrentImage.color, Color.clear, 10 * Time.deltaTime);
                index++;
            }
            index = 0;
            foreach (Text CurrentText in Texts)
            {
                CurrentText.color = Color.Lerp(CurrentText.color, Color.clear, 10 * Time.deltaTime);
                index++;
            }
            yield return new WaitForEndOfFrame();
        }
        CoroutineRunning = false;
    }
}
