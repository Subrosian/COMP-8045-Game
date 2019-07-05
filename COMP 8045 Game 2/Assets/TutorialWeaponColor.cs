using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialWeaponColor : MonoBehaviour {

    // Use this for initialization
    void Start() {
        Image currImage = GetComponent<Image>();
        switch (transform.name) {
            case "LaserWeaponImg":
                    currImage.color = Shop.colorWithCurrAlpha(currImage.color, Shop.shopUnselectedUnobtainedWeaponColor);
                break;
            case "CrossbowImg":
                    currImage.color = Shop.colorWithCurrAlpha(currImage.color, Shop.shopUnselectedUnobtainedWeaponColor);
                break;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
