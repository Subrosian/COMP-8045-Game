using UnityEngine;
using System.Collections;

public class DayNightCycle : MonoBehaviour {


    //Jacob's note: The Day/night cycle is included as part of the stage in order to conform to the game's theme. It's a basic cycle. It could be considered as polish.
    //It's not exactly like a normal day/night cycle since the intensity of the darkness is linearly related to the position of the sine wave, rather than as part of a directional light as its relation to the position of the sine wave. Still, I think it's good enough.

    public static float SecondsInDay = 20f; //noting such and modifying such for later as well ...
    public float maxAlpha = 99f;
    public static float currTime = 0;

    public static bool isDay
    {
        get
        {
            return currTime > SecondsInDay / 2; //when the -sin function of the time of day is negative, then it is day
        }
    }

	// Use this for initialization
	void Start () {
        Color currColor = GetComponent<SpriteRenderer>().color;
        if (WaveManager.isShadowMode)
        {
            currColor = new Color(26 / 255f, 25 / 255f, 0f, currColor.a); //Shadow mode shade color
        }
        currTime = (currTime + Time.fixedDeltaTime) % (SecondsInDay);
        GetComponent<SpriteRenderer>().color = new Color(currColor.r, currColor.g, currColor.b, (maxAlpha / 2f / 255) + (maxAlpha / 2f / 255) * -Mathf.Sin(currTime * Mathf.PI * 2 / SecondsInDay));
    }

    // Update is called once per frame
    void Update () {
        Color currColor = GetComponent<SpriteRenderer>().color;
        currTime = (currTime + Time.fixedDeltaTime)%(SecondsInDay);
        GetComponent<SpriteRenderer>().color = new Color(currColor.r, currColor.g, currColor.b, ((1.5f*maxAlpha)/2f / 255) + (maxAlpha/3f / 255) * -Mathf.Sin(currTime * Mathf.PI * 2 / SecondsInDay));
	}
}
