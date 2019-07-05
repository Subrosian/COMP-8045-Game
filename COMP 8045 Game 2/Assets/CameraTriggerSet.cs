using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTriggerSet : MonoBehaviour {

    Camera cam;
    public static float camHeight, camWidth; //height and width of orthographic camera

    // Use this for initialization
    void Start () {
        cam = Camera.main;
        camHeight = 2f * cam.orthographicSize;
        camWidth = camHeight * cam.aspect;
        GetComponent<BoxCollider2D>().size = new Vector2(camWidth, camHeight);
    }

    // Update is called once per frame
    void Update () {		
	}
}
