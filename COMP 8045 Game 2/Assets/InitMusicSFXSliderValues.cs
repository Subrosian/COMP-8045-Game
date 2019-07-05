using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InitMusicSFXSliderValues : MonoBehaviour {

	// Use this for initialization
	void Start () {
		switch(name)
        {
            case "MusicVolSlider":
                GetComponent<Slider>().value = MusicSFXVolChange.MusicVol;
                break;
            case "SFXVolSlider":
                GetComponent<Slider>().value = MusicSFXVolChange.SoundVol;
                break;
            default:
                break;
        }
	}
	
	// Update is called once per frame
	void Update () {
	}
}
