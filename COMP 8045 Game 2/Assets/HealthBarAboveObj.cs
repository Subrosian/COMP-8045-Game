using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class HealthBarAboveObj : MonoBehaviour {

    Camera MainCamera;
    private GameObject HealthBarOrig;
    public GameObject HealthBar;
    public Transform ParentTransform; //game object containing the canvas
    Vector3 offsetFromChar = new Vector3(0, 0.7f, 0);

	void Start () { //to be done after ShadowHealth would be run
        HealthBarOrig = GameObject.Find("ShadowHealthSliderOrig");
        ParentTransform = HealthBarOrig.transform.parent;
        MainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        HealthBar = Instantiate(HealthBarOrig);
        HealthBar.transform.SetParent(ParentTransform, false);
        //initialize health
        HealthBar.GetComponent<Slider>().value = GetComponent<ShadowHealth>().currHealth;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        HealthBar.transform.position = MainCamera.WorldToScreenPoint(transform.position + offsetFromChar); //adjust health bar position
    }
}
