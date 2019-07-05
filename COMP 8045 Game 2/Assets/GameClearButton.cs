using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameClearButton : MonoBehaviour, IPointerClickHandler
{
    public AudioClip ButtonClickSound;
    public AudioClip CongratulationsThemeMusic;
    GameObject SingletonMusicObj, SingletonSoundObj;

    // Use this for initialization
    void Start () {
        //Button also includes the initial actions on the Game Clear scene and not just button behaviour
        PlayerPrefs.SetInt("ShadowModeUnlocked", 1);
        PlayerPrefs.DeleteKey("currentLevel"); //used to end the playthrough

        SingletonMusicObj = GameObject.Find("SingletonMusicAudioSource");
        SingletonSoundObj = GameObject.Find("SingletonSoundAudioSource");
        AudioSource MusicAudioSource = SingletonMusicObj.GetComponent<AudioSource>();
        MusicAudioSource.clip = CongratulationsThemeMusic; //intentionally setting the main menu music to continue with the congratulations theme music, as a 'music change easter egg'
        MusicAudioSource.Stop();
        MusicAudioSource.Play();
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log("name: "+name);
        switch (name)
        {
            case "CongratulationsEndButton":
                AudioSource SoundAudioSource = SingletonSoundObj.GetComponent<AudioSource>();
                SoundAudioSource.clip = ButtonClickSound;
                SoundAudioSource.PlayOneShot(ButtonClickSound);
                OnLoadTransition.LoadScene("TitleScreenScene");
                break;
            default:
                break;
        }
    }
}
