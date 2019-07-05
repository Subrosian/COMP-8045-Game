using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopExclusiveTarget : MonoBehaviour {

    public Text textTarget;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Shop.isActive)
        {
            textTarget.raycastTarget = true;
        }
        else
        {
            textTarget.raycastTarget = false;
        }
	}
}
