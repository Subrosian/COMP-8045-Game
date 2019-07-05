using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadPMDataButton : MonoBehaviour, IPointerClickHandler
{

    float buttonPressedTimer;
    const float buttonPressedCooldown = 1f;
    Color initColor, diffColor;
    public bool toSave, toLoad;

    // Use this for initialization
    void Start () {
        buttonPressedTimer = 0f;
        initColor = GetComponent<Image>().color;
        diffColor = new Color(0f, 0f, 0f, 1f);
    }
	
	// Update is called once per frame
	void Update () {
        if(buttonPressedTimer > 0f)
        {
            buttonPressedTimer -= Time.deltaTime;
        }
        else
        {
            GetComponent<Image>().color = initColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(buttonPressedTimer > 0f) //don't do anything if cooldown hasn't finished
        {
            return;
        }
        buttonPressedTimer = buttonPressedCooldown;
        GetComponent<Image>().color = diffColor;

        //Do save/load
        if (toSave)
        {
            ShadowPM.savingData = true;
        }
        if(toLoad)
        {
            foreach (GameObject shadowCharacter in GameObject.FindGameObjectsWithTag("ShadowCharacter"))
            {
                shadowCharacter.GetComponent<ShadowPM>().loadingData = true;
            }
        }
    }
}
