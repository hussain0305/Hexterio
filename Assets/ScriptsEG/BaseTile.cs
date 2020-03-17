using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class BaseTile : MonoBehaviour
{
    //** Class variables

    public SpriteRenderer TileSpriteRenderer;
    public Transform TileNeighborsMain;
    public GameObject ClockwiseRotationSprite;
    public GameObject Axle;
    public GameObject ShadowBP;
    public Vector2 ShadowOffset;
    public ValueScript ValueOnCanvas;
    public HighlightScript TileHighlight;
    public TileColors TileColorValues;

    public float TileRotationSpeed = 15;
    public TileType ThisTileType;
    public TileFlipAxis ThisTileFlipAxis;
    public TileRotation ThisTileRotation;
    public TileRoles ThisTileRole;
    public ValuePair TileValues;

    //** private Class variables

    private int CurrentValue;
    private int NumberOfNeighbors;
    private int FlipCount;
    private bool TileActive;
    private Vector3 CurrentRotation;
    private Vector3 OriginalPosition;
    private HexAngles HexAngleValues;
    private GameObject CanvasObject;
    private GameObject SpawnedShadow;
    private ValueScript TileText;
    private WorldManager Manager;
    private SpriteRenderer ShadowSpriteRenderer;
    private HighlightScript SpawnedHighlight;

    //** private Helper variables

    private Quaternion MidFlipRotation;
    private Quaternion FinalFlipRotation;


    void Awake()
    {
        switch (ThisTileType)
        {
            case TileType.Base:
                TileSpriteRenderer.color = TileColorValues.Base;
                break;
            case TileType.LevelStart:
                //TileSpriteRenderer.color = TileColorValues.LevelStart;
                break;
            case TileType.LevelEnd:
                TileSpriteRenderer.color = TileColorValues.LevelEnd;
                break;
            case TileType.Grouped:
                TileSpriteRenderer.color = TileColorValues.Grouped;
                break;
        }

        Manager = GameObject.FindObjectOfType<WorldManager>();
    }

    private void Start()
    {
        if (!Manager)
        {
            Manager = GameObject.FindObjectOfType<WorldManager>();
        }

        FlipCount = 0;
        CurrentRotation = transform.rotation.eulerAngles;
        OriginalPosition = transform.position;

        CanvasObject = GameObject.FindGameObjectWithTag("Canvas");
        CurrentValue = TileValues.FirstValue;
        NumberOfNeighbors = TileNeighborsMain.childCount;

        //Done here separately instead of calling Spawn Shadow function to ensure it isnt delayed due to function call
        SpawnedShadow = Instantiate(ShadowBP, new Vector3((transform.position.x + ShadowOffset.x), 
            (transform.position.y + ShadowOffset.y)), transform.rotation, Manager.GetCurrentLevelTransform());
        SpawnedShadow.transform.localScale = TileSpriteRenderer.gameObject.transform.localScale;
        ShadowSpriteRenderer = SpawnedShadow.GetComponent<SpriteRenderer>();

        switch (ThisTileRole)
        {
            case TileRoles.Rotating:
                ClockwiseRotationSprite.SetActive(true);
                break;
            case TileRoles.Double:
                //draw tile with x2
                break;
            case TileRoles.Triple:
                //draw tile with x3
                break;
            case TileRoles.Value:
                DisplayTileValue(ValueToDisplay.Both);
                break;
        }

        if (ThisTileRotation == TileRotation.AntiClockwise && ThisTileRole == TileRoles.Rotating)
        {
            ClockwiseRotationSprite.transform.localScale = new Vector3(-1 * ClockwiseRotationSprite.transform.localScale.x, ClockwiseRotationSprite.transform.localScale.y, ClockwiseRotationSprite.transform.localScale.z);//rotation = Quaternion.Euler(HexAngleValues.FullFlipVertical);
        }

        HexAngleValues.SetAngleValues();
        SetTileActive(false);
    }

    public TileType GetTileType()
    {
        return ThisTileType;
    }

    public TileRoles GetTileRole()
    {
        return ThisTileRole;
    }

    public void PlayerSelectedTile()
    {
        SetTileActive(false);
        Manager.SetCurrentTile(this);

        if (SpawnedHighlight)
        {
            Manager.HighlightActivity(SpawnedHighlight);
        }

        StartCoroutine(Manager.MovePawn());
        //Manager.QueueNewRotation(RotateTileSo.BackToOriginal, TileRotationReason.SteppedOn, this);

        if (ThisTileRole == TileRoles.Value)
        {
            SwitchValues();
        }
    }

    public void PlayerLeftTile()
    {
        SetTileActive(false);

        Manager.QueueNewRotation(RotateTileSo.FullFlip, TileRotationReason.SteppedOff, this);
    }

    public void TileHalfFlipOnTraversal()
    {
        //TileSpriteRenderer.color = TileColorValues.CurrentPosition;

        switch (ThisTileRole)
        {
            case TileRoles.Rotating:
                Manager.RotateThemTiles(this);
                break;
        }
    }

    public void TileHalfFlipAfterTraversal()
    {
        //check if here if it needs deletion or color change
        switch (ThisTileType)
        {
            case TileType.Base:
                if (FlipCount < 1)
                {
                    CurrentValue = TileValues.SecondValue;
                    if (ThisTileRole == TileRoles.Value)
                    {
                        //StartCoroutine(SwitchValues(ValueToDisplay.Second));
                        SwitchValues();
                    }
                }
                else
                {
                    DestroyTile();
                }
                TileSpriteRenderer.color = TileColorValues.Traversed;
                break;

            case TileType.LevelStart:
                DestroyTile();
                break;

            case TileType.LevelEnd:
                TileSpriteRenderer.color = TileColorValues.LevelEnd;
                break;

            case TileType.Grouped:
                TileSpriteRenderer.color = TileColorValues.Grouped;
                break;
        }
    }

    public bool GetTileActive()
    {
        return TileActive;
    }

    public void SetTileActive(bool Value)
    {
        TileActive = Value;
    }

    public int GetTileValue()
    {
        return CurrentValue;
    }

    public int GetFirstValue()
    {
        return TileValues.FirstValue;
    }

    public GameObject GetShadow()
    {
        return SpawnedShadow;
    }

    public void ShadowParented(bool Value)
    {
        if (Value)
        {
            SpawnedShadow.transform.SetParent(this.transform);
        }
        else
        {
            SpawnedShadow.transform.SetParent(this.transform.parent);
            SpawnedShadow.transform.position = new Vector2(this.transform.position.x, this.transform.position.y) + ShadowOffset;
        }
    }
    
    public void DisplayTileValue(ValueToDisplay ToDisplay)
    {
        if(ThisTileType == TileType.LevelStart)
        {
            return;
        }

        if (!TileText)
        {
            TileText = Instantiate(ValueOnCanvas, CanvasObject.transform);
            TileText.SetOwner(this);
        }

        TileText.transform.SetPositionAndRotation(Camera.main.WorldToScreenPoint(transform.position), Quaternion.Euler(new Vector3(0, 0, 0)));

        if (ThisTileType == TileType.LevelEnd)
        {
            TileText.GetComponent<Text>().text = "" + TileValues.FirstValue;
            if (ThisTileRole == TileRoles.Value)
            {
                TileText.GetComponent<Text>().text = "<color=white>" + TileText.GetComponent<Text>().text + "</color>";
            }
            return;
        }

        switch (ToDisplay)
        {
            case ValueToDisplay.Both:
                TileText.GetComponent<Text>().text = "<size=60>" + TileValues.FirstValue + "</size>|" + "<size=30>" + TileValues.SecondValue + "</size>";
                break;
            case ValueToDisplay.First:
                TileText.GetComponent<Text>().text = "" + TileValues.FirstValue;
                break;
            case ValueToDisplay.Second:
                TileText.GetComponent<Text>().text = "" + TileValues.SecondValue;
                break;
        }
    }

    public void DisplayNextTileValue()
    {
        if (FlipCount < 1)
        {
            DisplayTileValue(ValueToDisplay.First);
        }
        else
        {
            DisplayTileValue(ValueToDisplay.Second);
        }
        StartCoroutine(EnsureCorrectValue());
    }

    IEnumerator EnsureCorrectValue()
    {
        yield return new WaitForSeconds(0.5f);
        DisplayNextTileValue();
    }

    public int GetFlipCount()
    {
        return FlipCount;
    }

    public void FlipComplete(TileRotationReason TRR)
    {
        if (TRR == TileRotationReason.SteppedOn)
        {
            Manager.CommGM_TileSteppedOn(this);
        }
        else if (TRR == TileRotationReason.SteppedOff)
        {
            FlipCount++;
            Manager.CommGM_TileSteppedOff(this);
        }
    }

    public void SetupMove()
    {
        SetAllTilesInactive();
        StartCoroutine(CallActivateNeighbors());
    }

    IEnumerator CallActivateNeighbors()
    {
        yield return new WaitForSeconds(0.2f);
        ActivateNeighbors();
    }

    public void ActivateNeighbors()
    {
        bool flag = true;
        List<Collider2D> LocalList = new List<Collider2D>();
        foreach(Transform CurrentNeighbor in TileNeighborsMain)
        {
            CurrentNeighbor.GetComponent<CircleCollider2D>().OverlapCollider(new ContactFilter2D(), LocalList);
            foreach(Collider2D Current in LocalList)
            {
                if (Current.gameObject.GetComponent<BaseTile>())
                {
                    flag = false;
                    Current.gameObject.GetComponent<BaseTile>().SetTileActive(true);
                    Current.gameObject.GetComponent<BaseTile>().Highlight();
                }
            }
        }
        Manager.SetInputActive(true);

        if (flag)
        {
            Manager.LevelFailed(true);
        }
    }

    public void SetAllTilesInactive()
    {
        foreach(BaseTile CurrentTile in GameObject.FindObjectsOfType<BaseTile>())
        {
            CurrentTile.SetTileActive(false);
        }
    }

    public void DestroyTile()
    {
        Destroy(SpawnedShadow);
        Destroy(Axle);

        if (SpawnedHighlight)
        {
            Destroy(SpawnedHighlight.gameObject);
        }

        if (TileText)
        {
            Destroy(TileText.gameObject);
        }

        Manager.GetGameMode().TileWasDestroyed(this);

        Destroy(this.gameObject);
    }

    public void Highlight()
    {
        SpawnedHighlight = Instantiate(TileHighlight, transform.position, transform.rotation, Manager.GetCurrentLevelTransform());
        Manager.AddHighlightToList(SpawnedHighlight);
    }

    public void SwitchValues()
    {
        if (TileText)
        {
            TileText.GetComponent<Animator>().SetTrigger("TileFlash");
        }
    }

    public void RealignAxles()
    {
        Axle.transform.position = transform.position;
    }
}
