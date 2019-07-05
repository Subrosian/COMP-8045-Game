using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialSkillColor : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Image currImage = GetComponent<Image>();
        currImage.color = Shop.colorWithCurrAlpha(currImage.color, Shop.darkUnselectedSkillColor);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
