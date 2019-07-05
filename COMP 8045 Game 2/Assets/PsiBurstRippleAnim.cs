using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PsiBurstRippleAnim : MonoBehaviour {
    float elapsedTime;
    float animDuration;

	// Use this for initialization
	void Start () {
        elapsedTime = 0;
        animDuration = 3;
	}
	
	// Update is called once per frame
	void Update () {
		//expand ripples at a rate of 10 units per second
        foreach(Transform child in transform)
        {
            child.GetComponent<LineRendererEx>().setRadius(child.GetComponent<LineRendererEx>().radius + Time.deltaTime * 10);
            Color cColor = child.GetComponent<LineRendererEx>().color;
            float alphaFloat = child.GetComponent<LineRendererEx>().alphaFloat;

            alphaFloat -= Time.deltaTime * 255f / 255f; //decrease by 100% per second
            if (alphaFloat < 0)
                alphaFloat = 0;
            child.GetComponent<LineRendererEx>().alphaFloat = alphaFloat;
            child.GetComponent<LineRendererEx>().color = new Color(cColor.r, cColor.g, cColor.b, alphaFloat); //Note that the alpha would be frame dependent. Could save an internal alpha value as well.
            child.GetComponent<LineRendererEx>().updateColor();
        }
        elapsedTime += Time.deltaTime;
        if(elapsedTime >= animDuration)
        {
            Destroy(gameObject); //end the animation
        }
	}
}
