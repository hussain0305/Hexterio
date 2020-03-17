using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightScript : MonoBehaviour
{
    private SpriteRenderer SpriteRender;
    // Start is called before the first frame update
    void Start()
    {
        SpriteRender = GetComponent<SpriteRenderer>();
    }

    public void SelectedHighlight()
    {
        GetComponent<Animator>().SetTrigger("Selected");
    }

    public void DidntSelectHighlight()
    {
        GetComponent<Animator>().SetTrigger("NotSelected");
    }

    public void DestroyThyself()
    {
        Destroy(this.gameObject);
    }
}
