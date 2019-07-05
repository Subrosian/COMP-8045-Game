using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OutOfBoundsWarpBack : MonoBehaviour {

    GameObject player;
    bool doFadeFromBlack;
    public GameObject FadeFromBlackShade;
    float fadeFromBlackDuration = 1f;

	// Use this for initialization
	void Start () {
        player = GameObject.FindGameObjectWithTag("Player");
        FadeFromBlackShade.GetComponent<CanvasRenderer>().SetAlpha(0);
    }
	
	// Update is called once per frame
	void Update () {
        if (doFadeFromBlack)
        {
            Shop.DecrementImgAlphaProportionalToDuration(FadeFromBlackShade, fadeFromBlackDuration);
            if(FadeFromBlackShade.GetComponent<CanvasRenderer>().GetAlpha() <= 0) //end fade from black
            {
                doFadeFromBlack = false;
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.GetType() != typeof(BoxCollider2D))
        {
            return;
        }
        GameObject rootParent = other.gameObject;
        while (rootParent.transform.parent != null)
        {
            rootParent = rootParent.transform.parent.gameObject;
        }
        if (other.gameObject == player)
        {
            //warp player to center, do a fade from black to brighter, and damage player; this temporary obscured vision can also serve as punishment for going out of bounds
            player.transform.position = new Vector3(0, 0, player.transform.position.z);
            player.GetComponentInChildren<PlayerHealth>().TakeDamage(20);
            FadeFromBlackShade.GetComponent<CanvasRenderer>().SetAlpha(1f);
            doFadeFromBlack = true;
        }
        else if(other.gameObject.tag == "Enemy") //remove enemies, unless it is a boss - then warp it to the center
        {
            if (!(other.gameObject.GetComponentInChildren<EnemyHealth>().isBoss))
            {
                Destroy(rootParent); //destroy if it would be an enemy or whatever else
            }
            else
            {
                rootParent.transform.position = new Vector3(0, 0, rootParent.transform.position.z);
            }
        }
    }
}
