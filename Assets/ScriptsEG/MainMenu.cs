using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject StartContinue;
    public GameObject Levels;
    public GameObject AllModes;
    public GameObject LevelListScreen;
    public Color[] PassedColor;
    public Image BackgroundShade;
    public Color[] BackgroundShades;

    private int CurrentMode;
    private int CurrentLevel;
    private MenuManager MenuMan;

    // Start is called before the first frame update
    void Start()
    {
        CurrentMode = 0;
        CurrentLevel = 0;
        MenuMan = GameObject.FindObjectOfType<MenuManager>();

        //FETCH HERE SAVED VALUE OF PROGRESS - MODE AND LEVEL INSTEAD OF SETTING TO 0
        LevelListScreen.SetActive(false);

        StartCoroutine(FetchSavedValues());

    }

    public void StartContinueButton()
    {
        MenuMan.ThisButtonWasPressed(StartContinue, MenuButton.Continue);
    }

    public void LevelsButton()
    {
        MenuMan.ThisButtonWasPressed(Levels, MenuButton.Levels);
    }

    public void ShowLevelPage()
    {
        LevelListScreen.SetActive(true);
        StartCoroutine(MenuMan.FadeFader(FadeAction.LoadListOfLevels));
    }

    public void NextModeButton()
    {
        CurrentMode = (CurrentMode + 1) % AllModes.transform.childCount;
        ShowRelevantModePage();
        StartCoroutine(MenuMan.FadeFader(FadeAction.LoadListOfLevels));
    }

    public void PrevModeButton()
    {
        CurrentMode--;
        if (CurrentMode < 0)
        {
            CurrentMode += AllModes.transform.childCount;
        }
        StartCoroutine(MenuMan.FadeFader(FadeAction.LoadListOfLevels));
    }

    public void ShowRelevantModePage()
    {
        if (!LevelListScreen.activeSelf)
        {
            LevelListScreen.SetActive(true);
        }

        UpdateLevelIcons();

        foreach (Transform CM in AllModes.transform)
        {
            CM.gameObject.SetActive(false);
        }

        AllModes.transform.GetChild(CurrentMode).gameObject.SetActive(true);
    }

    public void UpdateLevelIcons()
    {
        //Current mode is stored in variable
        //cycle through all buttons
        int index = 0;
        Transform LevelList = AllModes.transform.GetChild(CurrentMode).transform.GetChild(2);

        foreach (Transform LevelIcon in LevelList)
        {
            if(GameController.GameControl.IsLevelUnlocked(CurrentMode, index))
            {
                if (GameController.GameControl.IsLevelCompleted(CurrentMode, index))
                {
                    LevelList.GetChild(index).GetChild(0).gameObject.SetActive(true);
                    LevelList.GetChild(index).GetChild(1).gameObject.SetActive(false);

                    LevelList.GetChild(index).GetComponent<Image>().color = PassedColor[CurrentMode];//Color.gray;
                }
                else
                {
                    LevelList.GetChild(index).GetChild(1).gameObject.SetActive(false);
                }
            }
            else
            {
                LevelList.GetChild(index).GetChild(0).gameObject.SetActive(false);
                LevelList.GetChild(index).GetChild(1).gameObject.SetActive(true);
            }
            index++;
        }
        BackgroundShade.color = BackgroundShades[CurrentMode];
    }

    public void LevelsButtonPressed(GameObject PressedButton)
    {
        string[] Parts = PressedButton.name.Split('(');
        string LevelString = Parts[1].Substring(0, Parts[1].Length - 1);
        Parts = PressedButton.transform.parent.transform.parent.name.Split('(');
        string ModeString = Parts[1].Substring(0, Parts[1].Length - 1);

        MenuMan.Manager.LoadLevelFromIndex(int.Parse(ModeString), int.Parse(LevelString));
    }

    public void PressedExit()
    {
        Application.Quit();
    }

    public void CreditsToggle()
    {
        GameObject.FindObjectOfType<MenuManager>().CreditsButton();
    }

    IEnumerator FetchSavedValues()
    {
        yield return new WaitForSeconds(0.1f);
        CurrentMode = GameController.GameControl.GetCurrentMode();
        CurrentLevel = GameController.GameControl.GetCurrentLevel();
    }

}
