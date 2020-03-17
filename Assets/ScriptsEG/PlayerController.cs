using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    //** Public class variables

    public GameObject Scoreboard;
    public Text ScoreText;

    //** Private class variables
    private WorldManager Manager;
    private BaseGameMode GameMode;

    private int TrueScore;
    private int ScoreOnDisplay;

    private bool ScoreCoroutineRunning;
    //** Private helper variables
    RaycastHit2D Hit;
    Touch Tap;

    // Start is called before the first frame update
    private void Awake()
    {
        Manager = GameObject.FindObjectOfType<WorldManager>();
        GameMode = GameObject.FindObjectOfType<BaseGameMode>();
    }

    void Start()
    {
        if (!Manager)
        {
            Manager = GameObject.FindObjectOfType<WorldManager>();
        }

        if (!GameMode)
        {
            GameMode = GameObject.FindObjectOfType<BaseGameMode>();
        }

        ScoreCoroutineRunning = false;
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if (Manager.GetInputActive())
            {
                Vector2 origin = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
                                          Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
                Hit = Physics2D.Raycast(origin, Vector2.zero, 0f);
                if (Hit && Hit.transform.gameObject.GetComponent<BaseTile>().GetTileActive())
                {
                    Manager.SetInputActive(false);
                    Manager.PlayerLeftTile();
                    StartCoroutine(SetPlayerOnTile(Hit.transform.gameObject.GetComponent<BaseTile>()));
                    //Hit.transform.gameObject.GetComponent<BaseTile>().PlayerSelectedTile();
                }
            }
        }
    }

    public void AddToScore(int AddAmount)
    {
        TrueScore += AddAmount;
        if (!ScoreCoroutineRunning)
        {
            StartCoroutine(ScoreFlip());
        }
    }
    
    public void ResetScores()
    {
        TrueScore = 0;
        ScoreOnDisplay = 0;
        ScoreText.text = "" + 0;
    }

    public void SetScoreboardState(bool NewState)
    {
        Scoreboard.SetActive(NewState);
    }

    IEnumerator SetPlayerOnTile(BaseTile CurrentTileToSet)
    {
        yield return new WaitForSeconds(0.2f);
        CurrentTileToSet.PlayerSelectedTile();
    }

    IEnumerator ScoreFlip()
    {
        ScoreCoroutineRunning = true;
        do
        {
            if (ScoreOnDisplay < TrueScore)
            {
                ScoreOnDisplay++;
            }
            else
            {
                ScoreOnDisplay--;
            }
            ScoreText.text = "" + ScoreOnDisplay;

            yield return new WaitForSeconds(0.1f);
        } while (ScoreOnDisplay != TrueScore);
        ScoreCoroutineRunning = false;
    }
}
