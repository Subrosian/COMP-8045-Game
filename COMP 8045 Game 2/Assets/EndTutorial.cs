﻿//using System.Collections;
//using UnityEngine.EventSystems;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class TutorialNextButton : MonoBehaviour, IPointerClickHandler {

//    public AudioSource TutorialAudioSource;
//    public AudioClip TutorialNextSound;

//    // Use this for initialization
//    void Start () {
		
//	}
	
//	// Update is called once per frame
//	void Update () {
		
//	}

//    public void OnPointerClick(PointerEventData eventData)
//    {
//        if(TutorialIntroPauseDisplay.isActive)
//        {
//            //play button sound for shop enter
//            TutorialAudioSource.clip = TutorialNextSound;
//            TutorialAudioSource.PlayOneShot(TutorialNextSound);

//            TutorialIntroPauseDisplay.isActive = false;
//        }
//    }
//}
