using UnityEngine;
using System.Collections;

public class OuterMovePart : MonoBehaviour {

    public static float GameObj_Radius;
    public static Vector3 OuterMovePart_Pos; //position of this "outer move part" in world coordinates

    // Use this for initialization
    void Start () {
        GameObj_Radius = GetComponent<RectTransform>().rect.width * transform.localScale.x / 2;
        OuterMovePart_Pos = transform.position;
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
