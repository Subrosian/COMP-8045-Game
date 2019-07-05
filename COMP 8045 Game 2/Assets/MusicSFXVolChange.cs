using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicSFXVolChange : MonoBehaviour {

    public Slider musicVolSlider, soundVolSlider;

    public static float MusicVol
    {
        get
        {
            if (PlayerPrefs.HasKey("MusicVol"))
            {
                return PlayerPrefs.GetFloat("MusicVol");
            }
            else
            {
                return 0.7f; //default
            }
        }
        set { PlayerPrefs.SetFloat("MusicVol", value); }
    }
    public static float SoundVol
    {
        get
        {
            if (PlayerPrefs.HasKey("SoundVol"))
            {
                return PlayerPrefs.GetFloat("SoundVol");
            }
            else
            {
                return 0.7f; //default
            }
        }
        set { PlayerPrefs.SetFloat("SoundVol", value); }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnMusicChange()
    {
        MusicVol = musicVolSlider.value;
        Debug.Log("MusicVol: " + MusicVol);
    }
    public void OnSoundChange()
    {
        SoundVol = soundVolSlider.value;
    }
}
