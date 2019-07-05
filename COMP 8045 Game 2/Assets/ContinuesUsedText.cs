using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContinuesUsedText : MonoBehaviour {

    public static bool updateText = false; //doing this as opposed to calling a function from a game object that would be found by name of the game object
    //because there would be only one ContinuesUsedText that such would be used for as of writing this code on 2/7/19

    private void Awake()
    {
        updateText = false;
    }

    // Use this for initialization
	void Start () {
        GetComponent<Text>().text = "RETRY CONTINUES USED: " + PlayerHealth.continueCount;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (updateText)
        {
            GetComponent<Text>().text = "RETRY CONTINUES USED: " + PlayerHealth.continueCount;
            updateText = false;
        }
    }
}
