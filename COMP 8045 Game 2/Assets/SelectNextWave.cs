﻿using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine;

public class SelectNextWave : MonoBehaviour, IPointerClickHandler
{

	// Use this for initialization
	void Start () {
		
	}

    public void OnPointerClick(PointerEventData eventData) //select gun with this event
    {
        if (Shop.isActive)
        {
            Shop.currSelectedShopItemGameObject = gameObject;
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
