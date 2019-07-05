using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialNextButton : MonoBehaviour, IPointerClickHandler
{

    public AudioSource TutorialAudioSource;
    public AudioClip TutorialNextSound;

    // Use this for initialization
    void Start()
    {
        TutorialIntroPauseDisplay.tutorialPage = 1;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (TutorialIntroPauseDisplay.isActive)
        {
            //play button sound for shop enter
            TutorialAudioSource.clip = TutorialNextSound;
            TutorialAudioSource.PlayOneShot(TutorialNextSound);

            TutorialIntroPauseDisplay.tutorialPage++;
            if(TutorialIntroPauseDisplay.tutorialPage == TutorialIntroPauseDisplay.numTutorialPages)
            {
                GetComponentInChildren<Text>().text = "START GAME";
            }
            if (TutorialIntroPauseDisplay.tutorialPage > TutorialIntroPauseDisplay.numTutorialPages)
            {
                TutorialIntroPauseDisplay.isActive = false;
            }
        }
    }
}
