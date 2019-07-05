using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverButton : MonoBehaviour, IPointerClickHandler {

    public AudioSource GameOverAudioSource;
    public AudioClip GameOverInitSound, GameOverClickSound;

	// Use this for initialization
	void Start () {
        //Button also includes the initial actions on the Game Over scene and not just button behaviour
        GameOverAudioSource.clip = GameOverInitSound;
        GameOverAudioSource.PlayOneShot(GameOverInitSound);
    }
	
	// Update is called once per frame
	void Update () {

    }
    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log("name: "+name);
        switch (name)
        {
            case "GameOverEndGameButton":
                WaveManager.continueFromGameOver = true;
                PlayerHealth.continueCount++;
                WaveManager.isNewGame = false;
                OnLoadTransition.LoadScene("TitleScreenScene");
                GameOverAudioSource.clip = GameOverClickSound;
                GameOverAudioSource.PlayOneShot(GameOverClickSound);
                break;
            default:
                break;
        }
    }
}
