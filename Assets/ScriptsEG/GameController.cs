using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

[System.Serializable]
public struct LevelUnlockData
{
    public int ModeNumber;
    public int LastLevelNumber;
}

public class GameController : MonoBehaviour
{
    public static GameController GameControl;

    //public AudioClip ButtonClick;
    public int CurrentMode;
    public int CurrentLevel;
    public LevelUnlockData[] UnlockedLevels;
    public LevelUnlockData[] CompletedLevels;

    //private AudioSource ButtonSound;

    private void Awake()
    {
        if (!GameControl)
        {
            DontDestroyOnLoad(gameObject);
            GameControl = this;
        }
        else if (GameControl != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UnlockedLevels = new LevelUnlockData[5];
        CompletedLevels = new LevelUnlockData[5];
        CurrentMode = 0;
        CurrentLevel = 0;

        InitializeCompletedLevels();

        //LoadGameStats();
        //ButtonSound = gameObject.AddComponent<AudioSource>();
        //ButtonSound.loop = false;
        //ButtonSound.clip = ButtonClick;
    }

    public void SaveGameStats()
    {
        BinaryFormatter BF = new BinaryFormatter();
        FileStream FS = File.Create(Application.persistentDataPath + "/HexSaveData.dat");

        PlayerData SessionData = new PlayerData();

        SessionData.CurrentMode = CurrentMode;
        SessionData.CurrentLevel = CurrentLevel;
        SessionData.UnlockedLevels = UnlockedLevels;
        SessionData.CompletedLevels = CompletedLevels;

        BF.Serialize(FS, SessionData);
        FS.Close();
    }

    public void LoadGameStats()
    {
        if (File.Exists(Application.persistentDataPath + "/HexSaveData.dat"))
        {
            BinaryFormatter BF = new BinaryFormatter();
            FileStream FS = File.Open(Application.persistentDataPath + "/HexSaveData.dat", FileMode.Open);

            PlayerData FetchedData = (PlayerData)BF.Deserialize(FS);

            FS.Close();

            CurrentMode = FetchedData.CurrentMode;
            CurrentLevel = FetchedData.CurrentLevel;
            UnlockedLevels = FetchedData.UnlockedLevels;
            CompletedLevels = FetchedData.CompletedLevels;
        }
    }
    
    /*
    public void PlayButtonSound()
    {
        if (!ButtonSound.isPlaying)
            ButtonSound.Play();
    }
    */

    public void InitializeCompletedLevels()
    {
        CompletedLevels[0].LastLevelNumber = -1;
        CompletedLevels[1].LastLevelNumber = -1;
        CompletedLevels[2].LastLevelNumber = -1;
        CompletedLevels[3].LastLevelNumber = -1;
        CompletedLevels[4].LastLevelNumber = -1;
    }

    public void NewLevelUnlocked(int ModeIndex, int LevelIndex)
    {
        if (UnlockedLevels[ModeIndex].LastLevelNumber < LevelIndex)
        {
            UnlockedLevels[ModeIndex].LastLevelNumber = LevelIndex;
            SaveGameStats();
        }
    }

    public void NewLevelCompleted(int ModeIndex, int LevelIndex)
    {
        if (CompletedLevels[ModeIndex].LastLevelNumber < LevelIndex)
        {
            CompletedLevels[ModeIndex].LastLevelNumber = LevelIndex;
            SaveGameStats();
        }
    }

    public void NewCurrentLevel(int Mode, int Level)
    {
        CurrentMode = Mode;
        CurrentLevel = Level;
        SaveGameStats();
    }

    public int GetCurrentMode()
    {
        LoadGameStats();
        return CurrentMode;
    }

    public int GetCurrentLevel()
    {
        LoadGameStats();
        return CurrentLevel;
    }

    public LevelUnlockData[] GetUnlockedLevels()
    {
        LoadGameStats();
        return UnlockedLevels;
    }

    public LevelUnlockData[] GetCompletedLevels()
    {
        LoadGameStats();
        return CompletedLevels;
    }

    public bool IsLevelUnlocked(int ModeIndex, int LevelIndex)
    {
        LoadGameStats();
        if (UnlockedLevels[ModeIndex].LastLevelNumber >= LevelIndex)
        {
            return true;
        }

        return false;
    }

    public bool IsLevelCompleted(int ModeIndex, int LevelIndex)
    {
        LoadGameStats();
        if (CompletedLevels[ModeIndex].LastLevelNumber >= LevelIndex)
        {
            return true;
        }

        return false;
    }

}

[Serializable]
class PlayerData
{
    public int CurrentMode;
    public int CurrentLevel;
    public LevelUnlockData[] UnlockedLevels;
    public LevelUnlockData[] CompletedLevels;
}