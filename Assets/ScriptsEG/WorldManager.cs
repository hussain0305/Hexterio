using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;


//** Struct and Enum declarations

public struct HexAngles
{
    public Vector3 DefaultAngle;
    public Vector3 Clockwise;
    public Vector3 AntiClockwise;
    public Vector3 HalfFlipVertical;
    public Vector3 FullFlipVertical;
    public Vector3 HalfFlipHorizontal;
    public Vector3 FullFlipHorizontal;

    public void SetAngleValues()
    {
        DefaultAngle = new Vector3(0, 0, 0);
        Clockwise = new Vector3(0, 0, -60);
        AntiClockwise = new Vector3(0, 0, 60);
        HalfFlipVertical = new Vector3(0, -90, 0);
        FullFlipVertical = new Vector3(0, -180, 0);
        HalfFlipHorizontal = new Vector3(90, 0, 0);
        FullFlipHorizontal = new Vector3(180, 0, 0);
    }
}

public struct RotationOrder
{
    public TileRotationReason TileRR;
    public BaseTile TileToRotate;
    public RotateTileSo HowToRotate;

    public RotationOrder(TileRotationReason TTRR, BaseTile TTileToRotate, RotateTileSo THowToRotate)
    {
        TileRR = TTRR;
        TileToRotate = TTileToRotate;
        HowToRotate = THowToRotate;
    }
}

[System.Serializable]
public struct ValuePair
{
    public int FirstValue;
    public int SecondValue;
}

[System.Serializable]
public struct TileColors
{
    public Color Base;
    public Color LevelEnd;
    public Color Traversed;
    public Color Grouped;
}

public enum RotateTileSo { BackToOriginal, FullFlip, Clockwise, AntiClockwise }
public enum TileType { Base, Grouped, LevelStart, LevelEnd };
public enum TileFlipAxis { Horizontal, Vertical };
public enum TileRotation { Clockwise, AntiClockwise };
public enum TileRoles { Blank, Value, Double, Triple, Rotating };
public enum TileRotationReason { SteppedOff, SteppedOn, Remote};
public enum MenuButton { Continue, Levels, Credits };
public enum ValueToDisplay { Both, First, Second};

public class WorldManager : MonoBehaviour
{
    //**Public class variables

    public Transform Levels;
    public GameObject LevelDetails;
    public MenuManager MenuMan;
    public GameObject CurrentPlayerPosition;

    //** Private class variables

    private int CurrentModeNumber;
    private int CurrentLevelNumber;
    private bool InputActive = true;
    private bool RotationQueued;
    private bool UngroupingPending;
    private Vector3 OriginalDimensions_PositionMarker;
    private Queue<RotationOrder> QueuedRotations;
    private BaseTile CurrentTile;
    private Transform CurrentMode;
    private HexAngles HexAngleValues;
    private GameObject CurrentLevel;
    private GameObject CurrentLevelCopy;
    private GameObject AllAxles;
    private BaseGameMode GameMode;
    private Queue<HighlightScript> AllHighlights;

    //** Class helper variables
    Quaternion MidFlipRotation;
    Quaternion FinalFlipRotation;

    // Start is called before the first frame update
    void Start()
    {
        CurrentModeNumber = GameController.GameControl.GetCurrentMode();
        CurrentLevelNumber = GameController.GameControl.GetCurrentLevel();
        RotationQueued = false;

        QueuedRotations = new Queue<RotationOrder>();
        HexAngleValues.SetAngleValues();

        CurrentModeNumber = GameController.GameControl.CurrentMode;
        CurrentLevelNumber = GameController.GameControl.CurrentLevel;

        AllHighlights = new Queue<HighlightScript>();
    }

    public void StartGame()
    {
        CurrentPlayerPosition = Instantiate(CurrentPlayerPosition);
        OriginalDimensions_PositionMarker = CurrentPlayerPosition.transform.localScale;
        LoadMode();
    }

    public void LoadMode()
    {
        MenuMan.SetRestartButtonState(true);
        MenuMan.SetMainMenuButtonState(true);
        MenuMan.SetVolumeButtonState(true);
        MenuMan.SetChangeMusicButtonState(true);

        if (CurrentModeNumber < Levels.childCount)
        {
            if (CurrentMode)
            {
                CurrentMode.gameObject.SetActive(false);
            }
            CurrentMode = Levels.GetChild(CurrentModeNumber);
            CurrentMode.gameObject.SetActive(true);
            GameMode = CurrentMode.GetComponent<BaseGameMode>();

            LoadLevel();
        }
    }

    public void LoadLevel()
    {
        SetInputActive(false);
        GameMode.ResetLocalLevelAtributes();

        if (CurrentLevelNumber < CurrentMode.childCount)
        {
            CurrentLevel = CurrentMode.GetChild(CurrentLevelNumber).gameObject;

            CurrentLevelCopy = Instantiate(CurrentLevel);

            CurrentLevel.SetActive(true);

            GameMode.NewLevelStarted(CurrentLevel);
            SetCurrentTile(CurrentLevel.GetComponent<LevelManager>().GetLevelStart());

            CurrentPlayerPosition.transform.position = CurrentLevel.GetComponent<LevelManager>().GetLevelStart().transform.position;

            GameController.GameControl.NewLevelUnlocked(GetModeIndex(), GetLevelIndex());
            GameController.GameControl.NewCurrentLevel(GetModeIndex(), GetLevelIndex());

            ShowLevelDetails(CurrentModeNumber, CurrentLevelNumber);
            ScalePositionMarker(CurrentLevel);
            RemoveAxles();
            NextMovePrep();
        }
    }

    public void ShowLevelDetails(int ModeNo, int LevelNo)
    {
        LevelDetails.SetActive(true);
        foreach(Transform CurrMode in LevelDetails.transform)
        {
            foreach(Transform CurrLevel in CurrMode)
            {
                CurrLevel.gameObject.SetActive(false);
            }
        }

        LevelDetails.transform.GetChild(ModeNo).GetChild(LevelNo).gameObject.SetActive(true);
    }

    public void ScalePositionMarker(GameObject ReferenceLevel)
    {
        CurrentPlayerPosition.transform.localScale = new Vector3(OriginalDimensions_PositionMarker.x * ReferenceLevel.transform.localScale.x,
            OriginalDimensions_PositionMarker.y * ReferenceLevel.transform.localScale.y,
            OriginalDimensions_PositionMarker.z * ReferenceLevel.transform.localScale.z);
    }

    public void SetInputActive(bool Value)
    {
        InputActive = Value;
    }

    public bool GetInputActive()
    {
        return InputActive;
    }

    public void PlayerLeftTile()
    {
        CurrentTile.PlayerLeftTile();
    }

    public BaseGameMode GetGameMode()
    {
        return GameMode;
    }

    public void CommGM_TileSteppedOn(BaseTile Tile)
    {
        GameMode.TileWasSteppedOn(Tile);
    }

    public void CommGM_TileSteppedOff(BaseTile Tile)
    {
        GameMode.TileWasSteppedOff(Tile);
    }

    public void EndThisLevel()
    {
        CurrentLevel.GetComponent<LevelManager>().LevelAchievementCheck();
        MenuMan.SetAxleButtonState(false);
        GameController.GameControl.NewLevelCompleted(GetModeIndex(), GetLevelIndex());

        foreach (GameObject ThisText in GameObject.FindGameObjectsWithTag("TileText"))
        {
            Destroy(ThisText);
        }
        CurrentLevel.SetActive(false);
        ProcessNextLevel();
    }

    public void ProcessNextLevel()
    {
        CurrentLevelNumber++;
        if (CurrentLevelNumber < CurrentMode.childCount)
        {
            LoadLevel();
        }
        else
        {
            ModeCompleted(CurrentModeNumber);
            CurrentModeNumber++;
            CurrentLevelNumber = 0;
            if(CurrentModeNumber < Levels.childCount)
            {
                LoadMode();
            }
            else
            {
                //All mode are done. Game Complete
            }
        }
    }

    public void MakeLevelCopy()
    {
        if (CurrentLevelCopy)
        {
            Destroy(CurrentLevelCopy);
        }

        CurrentLevelCopy = Instantiate(CurrentLevel);
        
    }

    public void ModeCompleted(int MNo)
    {
        string AchievementID = "HEX_MODE_" + MNo;
        SteamUserStats.SetAchievement(AchievementID);
        SteamUserStats.StoreStats();
        
        bool M0, M1, M2, M3, M4;
        SteamUserStats.GetAchievement("HEX_MODE_0", out M0);
        SteamUserStats.GetAchievement("HEX_MODE_1", out M1);
        SteamUserStats.GetAchievement("HEX_MODE_2", out M2);
        SteamUserStats.GetAchievement("HEX_MODE_3", out M3);
        SteamUserStats.GetAchievement("HEX_MODE_4", out M4);
        if (M0 && M1 && M2 && M3 && M4)
        {
            SteamUserStats.SetAchievement("HEX_ALL");
            SteamUserStats.StoreStats();
        }
    }

    public void RemoveAxles()
    {
        if (AllAxles)
        {
            Destroy(AllAxles);
        }
        AllAxles = Instantiate(new GameObject());
        AllAxles.name = "AllAxles";
        AllAxles.transform.parent = CurrentLevel.transform;

        foreach (Transform CurrTile in CurrentLevel.transform)
        {
            if (CurrTile.GetComponent<BaseTile>())
            {
                BaseTile LocalCopy = CurrTile.GetComponent<BaseTile>();
                LocalCopy.Axle.transform.parent = AllAxles.transform;

                if (LocalCopy.ThisTileFlipAxis == TileFlipAxis.Vertical)
                {
                    Vector3 Temp = new Vector3(0, 0, 90);
                    LocalCopy.Axle.transform.SetPositionAndRotation(LocalCopy.Axle.transform.position, Quaternion.Euler(Temp));
                }
            }
        }

        MenuMan.SetAllAxles(AllAxles);
    }

    public GameObject GetAllAxles()
    {
        return AllAxles;
    }

    public void RestartProceedings()
    {
        AllHighlights.Clear();
        MenuMan.RemoveTileTexts();

        CurrentLevelCopy.name = CurrentLevel.name;
        CurrentLevelCopy.transform.parent = CurrentLevel.transform.parent;
        int Temp = CurrentLevel.transform.GetSiblingIndex();
        Destroy(CurrentLevel);
        CurrentLevelCopy.transform.SetSiblingIndex(Temp);
        LoadLevel();
    }

    public int GetModeIndex()
    {
        return CurrentModeNumber;
    }

    public int GetLevelIndex()
    {
        return CurrentLevelNumber;
    }

    public void LoadLevelFromIndex(int Mode, int Level)
    {
        if (!LevelPermittedForPlayer(Mode, Level))
        {
            return;
        }

        DisableAllLevels();

        if (Mode < Levels.childCount)
        {
            CurrentModeNumber = Mode;
            CurrentLevelNumber = Level;
            StartCoroutine(MenuMan.FadeFader(FadeAction.LoadCurrentLevel));
        }
    }

    public bool LevelPermittedForPlayer(int Mode, int Level)
    {
        LevelUnlockData[] T_UnlockData = GameController.GameControl.GetUnlockedLevels();
        return (Level <= T_UnlockData[Mode].LastLevelNumber) ? true : false;
    }

    public void DisableAllLevels()
    {
        foreach(Transform ModeCurrent in Levels)
        {
            foreach(Transform LevelCurrent in ModeCurrent.transform)
            {
                LevelCurrent.gameObject.SetActive(false);
            }
            ModeCurrent.gameObject.SetActive(false);
        }
    }

    public void SetCurrentTile(BaseTile NewTile)
    {
        CurrentTile = NewTile;
    }

    public BaseTile GetCurrentTile()
    {
        return CurrentTile;
    }

    public Transform GetCurrentLevelTransform()
    {
        return CurrentLevel.transform;
    }

    public int GetCurrentModeNumber()
    {
        return CurrentModeNumber;
    }

    public void NextMovePrep()
    {
        if(CurrentLevel.GetComponent<LevelManager>().GetLevelEnd() == CurrentTile)
        {
            if (GameMode.HasConditionBeenMet())
            {
                StartCoroutine(MenuMan.FadeFader(FadeAction.LoadNextLevel));
            }
            else
            {
                MenuMan.LevelFailed(false);
            }
        }
        else
        {
            CurrentTile.SetupMove();
        }
    }

    public void LevelFailed(bool Value)
    {
        MenuMan.LevelFailed(Value);
    }

    public void GroupTiles(BaseTile ParentTile)
    {
        List<GameObject> AllGrouped = new List<GameObject>();

        foreach(Transform CurrTile in CurrentLevel.transform)
        {
            if (CurrTile.GetComponent<BaseTile>() && CurrTile.GetComponent<BaseTile>().GetTileType() == TileType.Grouped && ParentTile != CurrTile)
            {
                AllGrouped.Add(CurrTile.gameObject);
            }
        }

        foreach (GameObject CurrTile in AllGrouped)
        {
            CurrTile.transform.SetParent(ParentTile.transform);
            CurrTile.GetComponent<BaseTile>().ShadowParented(true);
        }

        BindPositionToTile(true);
    }

    public void UngroupTiles(BaseTile ParentTile)
    {
        List<GameObject> AllGrouped = new List<GameObject>();

        foreach (Transform CurrentChild in ParentTile.transform)
        {
            if (CurrentChild.GetComponent<BaseTile>())
            {
                AllGrouped.Add(CurrentChild.gameObject);
            }
        }

        foreach(GameObject CurrTile in AllGrouped)
        {
            CurrTile.transform.parent = ParentTile.transform.parent;
            CurrTile.GetComponent<BaseTile>().ShadowParented(false);
            CurrTile.GetComponent<BaseTile>().RealignAxles();
        }

        BindPositionToTile(false);
    }

    public void QueueNewRotation(RotateTileSo HowToRotate, TileRotationReason TRR, BaseTile TileToRotate)
    {
        RotationOrder NewRotation = new RotationOrder(TRR, TileToRotate, HowToRotate);

        QueuedRotations.Enqueue(NewRotation);
    }

    public void StartNewRotation()
    {
        if (QueuedRotations.Count > 0)
        {
            RotationQueued = true;
            RotationOrder OrderToPlace = QueuedRotations.Dequeue();
            StartCoroutine(CentralMakeTileRotate(OrderToPlace.HowToRotate, OrderToPlace.TileRR, OrderToPlace.TileToRotate));
        }

        else
        {
            RotationQueued = false;
            NextMovePrep();
        }
    }

    public void AddHighlightToList(HighlightScript HighlightToAdd)
    {
        AllHighlights.Enqueue(HighlightToAdd);
    }

    public void HighlightActivity(HighlightScript SelectedHighlight)
    {
        HighlightScript CurrHighlight;
        while (AllHighlights.Count > 0)
        {
            CurrHighlight = AllHighlights.Dequeue();
            if (CurrHighlight == SelectedHighlight)
            {
                CurrHighlight.SelectedHighlight();
            }
            else
            {
                CurrHighlight.DidntSelectHighlight();
            }
        }
    }

    public void BindPositionToTile(bool Value)
    {
        if (Value)
        {
            CurrentPlayerPosition.transform.SetParent(CurrentTile.transform);
        }
        else
        {
            CurrentPlayerPosition.transform.parent = null;
        }
    }

    public void RotateThemTiles(BaseTile TileToExclude)
    {
        foreach(Transform CurrentChild in CurrentLevel.transform)
        {
            if (CurrentChild.GetComponent<BaseTile>() && CurrentChild.GetComponent<BaseTile>().GetTileRole()==TileRoles.Rotating &&
                CurrentChild.gameObject != TileToExclude.gameObject)
            {
                if (CurrentChild.GetComponent<BaseTile>().ThisTileRotation == TileRotation.Clockwise)
                {
                    QueueNewRotation(RotateTileSo.Clockwise, TileRotationReason.Remote, CurrentChild.GetComponent<BaseTile>());
                }
                else
                {
                    QueueNewRotation(RotateTileSo.AntiClockwise, TileRotationReason.Remote, CurrentChild.GetComponent<BaseTile>());
                }
            }
        }
    }

    public IEnumerator MovePawn()
    {
        while(Vector2.Distance(CurrentPlayerPosition.transform.position, CurrentTile.transform.position) > 0.05f)
        {
            CurrentPlayerPosition.transform.position = Vector2.Lerp(CurrentPlayerPosition.transform.position, CurrentTile.transform.position, 10 * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        CurrentPlayerPosition.transform.position = CurrentTile.transform.position;
        QueueNewRotation(RotateTileSo.BackToOriginal, TileRotationReason.SteppedOn, CurrentTile);

        StartNewRotation();
    }

    public IEnumerator CallSetInputActive(bool Value)
    {
        yield return new WaitForSeconds(0.2f);
        SetInputActive(Value);
    }

    public IEnumerator CallNextMovePrep()
    {
        yield return new WaitForSeconds(0.2f);
        NextMovePrep();
    }

    public IEnumerator CentralMakeTileRotate(RotateTileSo HowToRotate, TileRotationReason TRR, BaseTile TileToRotate)
    {
        UngroupingPending = false;
        if (TRR != TileRotationReason.SteppedOn && TileToRotate.GetTileType() == TileType.Grouped && CurrentTile.GetTileType()!=TileType.Grouped)
        {
            GroupTiles(TileToRotate);
            UngroupingPending = true;
            yield return new WaitForSeconds(0.1f);

        }

        GameObject Shadow = TileToRotate.GetShadow();

        switch (HowToRotate)
        {
            case RotateTileSo.BackToOriginal:
                switch (TileToRotate.ThisTileFlipAxis)
                {
                    case TileFlipAxis.Horizontal:
                        MidFlipRotation = TileToRotate.transform.rotation;
                        FinalFlipRotation = TileToRotate.transform.rotation;
                        break;

                    case TileFlipAxis.Vertical:
                        MidFlipRotation = TileToRotate.transform.rotation;
                        FinalFlipRotation = TileToRotate.transform.rotation;
                        //MidFlipRotation = Quaternion.Euler(TileToRotate.transform.rotation.eulerAngles + HexAngleValues.HalfFlipVertical);
                        //FinalFlipRotation = Quaternion.Euler(TileToRotate.transform.rotation.eulerAngles);
                        break;
                }
                break;

            case RotateTileSo.FullFlip:
                switch (TileToRotate.ThisTileFlipAxis)
                {
                    case TileFlipAxis.Horizontal:
                        MidFlipRotation = Quaternion.Euler(TileToRotate.transform.rotation.eulerAngles + HexAngleValues.HalfFlipHorizontal);
                        FinalFlipRotation = Quaternion.Euler(TileToRotate.transform.rotation.eulerAngles + HexAngleValues.FullFlipHorizontal);
                        break;

                    case TileFlipAxis.Vertical:
                        MidFlipRotation = Quaternion.Euler(TileToRotate.transform.rotation.eulerAngles + HexAngleValues.HalfFlipVertical);
                        FinalFlipRotation = Quaternion.Euler(TileToRotate.transform.rotation.eulerAngles + HexAngleValues.FullFlipVertical);
                        break;
                }
                break;

            case RotateTileSo.Clockwise:
                FinalFlipRotation = Quaternion.Euler(TileToRotate.transform.rotation.eulerAngles + HexAngleValues.Clockwise);
                break;

            case RotateTileSo.AntiClockwise:
                FinalFlipRotation = Quaternion.Euler(TileToRotate.transform.rotation.eulerAngles + HexAngleValues.AntiClockwise);
                break;

        }

        if (HowToRotate != RotateTileSo.Clockwise && HowToRotate != RotateTileSo.AntiClockwise)
        {
            while (Quaternion.Angle(TileToRotate.transform.rotation, MidFlipRotation) > 2)
            {
                TileToRotate.transform.rotation = Quaternion.Lerp(TileToRotate.transform.rotation, MidFlipRotation, Time.deltaTime * TileToRotate.TileRotationSpeed);//* TileRotationSpeed
                Shadow.transform.rotation = Quaternion.Lerp(TileToRotate.transform.rotation, MidFlipRotation, Time.deltaTime * TileToRotate.TileRotationSpeed);//* TileRotationSpeed
                yield return new WaitForFixedUpdate();
            }

            TileToRotate.transform.rotation = MidFlipRotation;

            switch (TRR)
            {
                case TileRotationReason.SteppedOff:
                    TileToRotate.TileHalfFlipAfterTraversal();
                    break;
                case TileRotationReason.SteppedOn:
                    TileToRotate.TileHalfFlipOnTraversal();
                    break;
                case TileRotationReason.Remote:

                    break;
            }
        }

        yield return new WaitForSeconds(0.1f);

        if (TileToRotate)// && HowToRotate != RotateTileSo.BackToOriginal)
        {
            while (Quaternion.Angle(TileToRotate.transform.rotation, FinalFlipRotation) > 2)
            {
                TileToRotate.transform.rotation = Quaternion.Lerp(TileToRotate.transform.rotation, FinalFlipRotation, Time.deltaTime * TileToRotate.TileRotationSpeed);// 
                Shadow.transform.rotation = Quaternion.Lerp(TileToRotate.transform.rotation, FinalFlipRotation, Time.deltaTime * TileToRotate.TileRotationSpeed);// 
                yield return new WaitForFixedUpdate();
            }

            TileToRotate.transform.rotation = FinalFlipRotation;
            TileToRotate.FlipComplete(TRR);

        }

        if (UngroupingPending)
        {
            UngroupTiles(TileToRotate);
            yield return new WaitForSeconds(0.1f);
        }

        RotationQueued = false;
        StartNewRotation();
    }
}


//public int GetModeIndex()
//{
//    string[] Parts = CurrentMode.name.Split('(');
//    string ModeString = Parts[1].Substring(0, Parts[1].Length - 1);

//    return int.Parse(ModeString);
//}

//public int GetLevelIndex()
//{
//    string[] Parts = CurrentLevel.name.Split('(');
//    string LevelString = Parts[1].Substring(0, Parts[1].Length - 1);

//    return int.Parse(LevelString);
//}