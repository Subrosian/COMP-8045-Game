using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class ToggleTrainingUsing : MonoBehaviour, IPointerClickHandler
{

    bool useDiffColor;
    Color initColor, diffColor;

	// Use this for initialization
	void Start () {
        useDiffColor = false;
        initColor = GetComponent<Image>().color;
        diffColor = new Color(0f, 1f, 0f, 1f);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnPointerClick(PointerEventData eventData)
    {
        //toggle Training and Using here
        foreach(GameObject shadowCharacter in GameObject.FindGameObjectsWithTag("ShadowCharacter"))
        {
            shadowCharacter.GetComponent<ShadowPM>().toggleTrainingAndUsing = true;
        }
        //toggle color of this as well
        useDiffColor = !useDiffColor;
        if(useDiffColor)
        {
            GetComponent<Image>().color = diffColor;
            GetComponentInChildren<Text>().text = "U";
        }
        else
        {
            GetComponent<Image>().color = initColor;
            GetComponentInChildren<Text>().text = "T";
        }
    }
}
