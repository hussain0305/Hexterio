using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip[] BackgroudMusicList;
    public GameObject CustomFill;

    private int LastMusicIndex = -1;
    private AudioSource BackgroudMusic;

    public static MusicManager MusicMan;
    
    void Awake()
    {
        if (!MusicMan)
        {
            DontDestroyOnLoad(gameObject);
            MusicMan = this;
        }
        else if (MusicMan != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        BackgroudMusic = gameObject.AddComponent<AudioSource>();
        BackgroudMusic.loop = false;
        StartMusicClip();
    }

    public void StartMusicClip()
    {
        int MusicIndex = Random.Range(0, BackgroudMusicList.Length);
        if (MusicIndex != LastMusicIndex)
        {
            LastMusicIndex = MusicIndex;
            AudioClip Selected = BackgroudMusicList[MusicIndex];
            BackgroudMusic.clip = Selected;
            BackgroudMusic.Play();
            StartCoroutine(MusicCountdown(BackgroudMusic.clip.length));
        }
        else
        {
            StartMusicClip();
        }
        
    }

    public void VolumeChanged(float newVolume)
    {
        BackgroudMusic.volume = newVolume;
        if (!CustomFill)
        {
            CustomFill = GameObject.FindGameObjectWithTag("VolumeFill");
        }
        CustomFill.transform.localScale = new Vector3(newVolume, newVolume, newVolume);
    }

    IEnumerator MusicCountdown(float WaitTime)
    {
        yield return new WaitForSeconds(WaitTime);
        StartMusicClip();
    }
}
