using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlaying : MonoBehaviour {

    public AudioSource SoundPlayingAudioSource;
    
    //Credit for singleton pattern code for script goes to https://answers.unity.com/questions/11314/audio-or-music-to-continue-playing-between-scene-c.html
    private static SoundPlaying instance = null;
    public static SoundPlaying Instance
    {
        get { return instance; }
    }
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(this.gameObject);
    }
    // any other methods you need

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        SoundPlayingAudioSource.volume = MusicSFXVolChange.SoundVol;
    }
}
