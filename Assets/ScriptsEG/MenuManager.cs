using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/*
 * HOW THE UI IS ORGANIZED
 * Click On Continue == Hex expands -> Load level in the background -> Hex fades away -> Hex is deleted;
 * Click On Levels == Hex Expands -> Load Level list
 */

public enum FadeAction { CreateMainMenu, LoadCurrentLevel, LoadListOfLevels, LoadNextLevel, RestartLevel, BackToMenu}

public class MenuManager : MonoBehaviour
{
    public float ScaleSpeed;
    public float FadeSpeed;
    public Color WhiteColor;
    public Color ClearColor;
    public Vector3 FullCoverScale;

    public Image ScreenFader;
    public GameObject RestartLevelButton;
    public GameObject MainMenuButton;
    public GameObject AxlesButton;
    public GameObject VolumeButton;
    public GameObject ChangeMusicButton;
    public GameObject FailedScreen;

    public GameObject MainMenu;
    public WorldManager Manager;

    public Transform TopOfEverything;

    private GameObject InstantiatedMainMenu;
    private GameObject AllAxles;
    private GameObject LevelCanvas;

    private MusicManager MusicMan;

    private void Awake()
    {
        MusicMan = GameObject.FindGameObjectWithTag("MusicManager").GetComponent<MusicManager>();
    }

    void Start()
    {
        if (!Manager)
        {
            Manager = GameObject.FindObjectOfType<WorldManager>();
        }
        if (!MusicMan)
        {
            MusicMan = GameObject.FindGameObjectWithTag("MusicManager").GetComponent<MusicManager>();
        }

        LevelCanvas = GameObject.FindGameObjectWithTag("Canvas");

        SetRestartButtonState(false);
        SetMainMenuButtonState(false);
        SetAxleButtonState(false);
        SetVolumeButtonState(false);
        SetChangeMusicButtonState(false);
        StartCoroutine(FadeFader(FadeAction.CreateMainMenu));
    }

    public void CreateMainMenu()
    {
        InstantiatedMainMenu = Instantiate(MainMenu, LevelCanvas.transform);
        InstantiatedMainMenu.transform.SetAsFirstSibling();
        ScreenFader.transform.SetAsLastSibling();
    }

    public void CreditsButton()
    {
        FailedScreen.SetActive(true);
        FailedScreen.GetComponent<FlippingHexes>().CreditsToggle();
    }

    public void SetAllAxles(GameObject newAxle)
    {
        AllAxles = newAxle;
        AllAxles.SetActive(false);
        SetAxleButtonState(true);
    }

    public void PressedAxlesButton()
    {
        AllAxles.SetActive(!AllAxles.activeSelf);
    }

    public void PressedChangeMusicButton()
    {
        GameObject.FindObjectOfType<MusicManager>().StartMusicClip();
    }

    public void RestartButton()
    {
        StartCoroutine(FadeFader(FadeAction.RestartLevel));
    }

    public void PressedMainMenuButton()
    {
        StartCoroutine(FadeFader(FadeAction.BackToMenu));
    }

    public void SetRestartButtonState(bool newState)
    {
        RestartLevelButton.SetActive(newState);
    }
    
    public void SetAxleButtonState(bool newState)
    {
        AxlesButton.SetActive(newState);
    }

    public void SetMainMenuButtonState(bool newState)
    {
        MainMenuButton.SetActive(newState);
    }

    public void SetVolumeButtonState(bool newState)
    {
        VolumeButton.SetActive(newState);
    }

    public void SetChangeMusicButtonState(bool newState)
    {
        ChangeMusicButton.SetActive(newState);
    }

    public void ThisButtonWasPressed(GameObject ObjectToExpand, MenuButton PressedButton)
    {
        if (!InstantiatedMainMenu)
        {
            return;
        }

        switch (PressedButton)
        {
            case MenuButton.Continue:
                StartCoroutine(ScaleUp(ObjectToExpand, FadeAction.LoadCurrentLevel));
                break;
            case MenuButton.Levels:
                StartCoroutine(ScaleUp(ObjectToExpand, FadeAction.LoadListOfLevels));
                break;
        }
    }

    public void DestroyEverythingInOnTop()
    {
        foreach(Transform CurrentChild in TopOfEverything.transform)
        {
            Destroy(CurrentChild.gameObject);
        }
    }

    IEnumerator ScaleUp(GameObject ObjectToBeScaled, FadeAction ActionToBePerformed)
    {
        //make this the child of TopOfEverything
        ObjectToBeScaled.transform.SetParent(TopOfEverything);

        //detach child from gameobject
        ObjectToBeScaled.transform.DetachChildren();

        while (ObjectToBeScaled.transform.localScale.x < FullCoverScale.x)
        {
            ObjectToBeScaled.transform.localScale = Vector3.Lerp(ObjectToBeScaled.transform.localScale, (2 * FullCoverScale), Time.deltaTime * ScaleSpeed);
            yield return new WaitForEndOfFrame();
        }

        StartCoroutine(FadeFader(ActionToBePerformed)); //FadeAction.LoadCurrentLevel
    }

    public IEnumerator FadeFader(FadeAction Action)
    {
        Color FaderInitialColor;
        Color FaderFinalColor;

        ScreenFader.gameObject.SetActive(true);
        ScreenFader.transform.SetAsLastSibling();
        FaderInitialColor = ClearColor;
        FaderFinalColor = WhiteColor;

        ScreenFader.color = FaderInitialColor;
        
        while(ColorIsNotSimilar(ScreenFader.color, FaderFinalColor))
        {
            ScreenFader.color = Color.Lerp(ScreenFader.color, FaderFinalColor, FadeSpeed);
            yield return new WaitForEndOfFrame();
        }

        switch (Action)
        {
            case FadeAction.CreateMainMenu:
                CreateMainMenu();
                break;

            case FadeAction.LoadCurrentLevel:
                if (InstantiatedMainMenu)
                {
                    Destroy(InstantiatedMainMenu);
                }
                DestroyEverythingInOnTop();
                Manager.StartGame();

                break;

            case FadeAction.LoadListOfLevels:
                DestroyEverythingInOnTop();
                InstantiatedMainMenu.GetComponent<MainMenu>().ShowRelevantModePage();
                break;

            case FadeAction.LoadNextLevel:
                StartCoroutine(EnsureFaderState(0.1f, false));
                Manager.GetGameMode().LevelEnded();
                break;

            case FadeAction.RestartLevel:
                SetFailedScreenState(false);
                Manager.RestartProceedings();
                break;

            case FadeAction.BackToMenu:
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                break;
        }

        FaderInitialColor = WhiteColor;
        FaderFinalColor = ClearColor;

        ScreenFader.color = FaderInitialColor;

        //This is not completing
        while (ColorIsNotSimilar(ScreenFader.color, FaderFinalColor))
        {
            ScreenFader.color = Color.Lerp(ScreenFader.color, FaderFinalColor, FadeSpeed);
            yield return new WaitForEndOfFrame();
        }

        ScreenFader.gameObject.SetActive(false);
    }

    bool ColorIsNotSimilar(Color Subject, Color Desired)
    {
        if (Mathf.Abs(Subject.a - Desired.a) < 0.025)
        {
            return false;
        }

        return true;
    }

    public void SetFailedScreenState(bool state)
    {
        //FailedScreen.SetActive(state);
        if (state)
        {
            FailedScreen.SetActive(true);
            FailedScreen.GetComponent<FlippingHexes>().TurnScreenOn(true);
        }
        else
        {
            FailedScreen.GetComponent<FlippingHexes>().TurnScreenOff(true);
        }
    }

    public void LevelFailed(bool BecauseNoMovesLeft)
    {
        SetFailedScreenState(true);

        if (BecauseNoMovesLeft)
        {
            FailedScreen.transform.Find("Reason").GetComponent<Text>().text = "No moves available";
        }

        else
        {
            switch (Manager.GetCurrentModeNumber())
            {
                case 0:
                    FailedScreen.transform.Find("Reason").GetComponent<Text>().text = "Did not visit all tiles";
                    break;
                case 1:
                    FailedScreen.transform.Find("Reason").GetComponent<Text>().text = "Did not destroy all tiles";
                    break;
                case 2:
                    FailedScreen.transform.Find("Reason").GetComponent<Text>().text = "Total doesn't match the required value";
                    break;
            }
        }    
    }

    public void RemoveTileTexts()
    {
        foreach (Transform CurrText in LevelCanvas.transform)
        {
            if (CurrText.GetComponent<ValueScript>())
            {
                Destroy(CurrText.gameObject);
            }
        }
    }

    public void ProxyVolumeChanged(float newVolume)
    {
        if (!MusicMan)
        {
            MusicMan = GameObject.FindGameObjectWithTag("MusicManager").GetComponent<MusicManager>();
        }
        MusicMan.VolumeChanged(newVolume);
    }

    IEnumerator EnsureFaderState(float Duration, bool State)
    {
        yield return new WaitForSeconds(Duration);
        ScreenFader.gameObject.SetActive(State);
    }
}
