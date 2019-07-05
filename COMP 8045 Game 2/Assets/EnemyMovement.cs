using Pathfinding;
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class EnemyMovement : MonoBehaviour {

    //Transform player;

    //EnemyHealth enemyHealth;
    public float movementFreezeTimer;
    public float freezeColorTimer;
    private float prevColorTimer;
    private static Color freezeColor //made into a property in order to create a new instance for each time that it would be called?
    { 
        get { return new Color(241f / 255f, 255f / 255f, 0); }
    }

    // Use this for initialization
    void Start()
    {
        movementFreezeTimer = 0; //default
        freezeColorTimer = 0;

        if(WaveManager.isShadowMode)
        {
            float origMaxSpeed = GetComponent<AILerp>().speed;
            GetComponent<AILerp>().speed = origMaxSpeed * 1.3f; //Shadow mode difference - enemies move 30% faster. Bosses might be impossible without Temporal Inhibition, Delta Prison, and the Time Leap as a result, and could note also of enemies from this
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Prevent enemy from moving as long as movementFreezeTimer > 0 at the start of this Update call, as part of a freeze mechanic
        if (movementFreezeTimer < 0)
        {
            movementFreezeTimer = 0;
        }
        if(movementFreezeTimer == 0)
        {
            GetComponent<AILerp>().canMove = true;
            GetComponentInChildren<Animator>().enabled = true;
        }
        if (movementFreezeTimer > 0 || GetComponentInChildren<EnemyHealth>().isDead) //if dead, cease movement
        {
            GetComponent<AILerp>().canMove = false;
            GetComponentInChildren<Animator>().enabled = false;
            movementFreezeTimer -= Time.deltaTime;
        }
        if(freezeColorTimer < 0)
        {
            freezeColorTimer = 0;
        }
        if(freezeColorTimer == 0)
        {
            if (!GetComponentInChildren<EnemyHealth>().isDead) //not update color if dead, due to alpha fading
            {
                GetComponentInChildren<SpriteRenderer>().color = new Color(1, 1, 1);
            }
        }
        if(freezeColorTimer > 0) //color being active (used for Delta Prison freeze mechanic)
        {
            GetComponentInChildren<SpriteRenderer>().color = freezeColor;
            freezeColorTimer -= Time.deltaTime;
        }
        prevColorTimer = freezeColorTimer;
        
        if(WaveManager.fadeScreenIsActive)
        {
            GetComponent<AILerp>().canMove = false;
            GetComponentInChildren<Animator>().enabled = false;
        }

    }
}
