using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Profiling;

public class TouchMove : MonoBehaviour {
    public GameObject particleGameObj;

    public Vector3 prevMovePosition; //public in order to be used e.g. as a variable for enemy's Pursuit behaviour
    public Vector3 prevMoveDelta; //last movement before any collision would have been detected yet
     //public in order to be used e.g. as a variable for enemy's Pursuit behaviour
    bool isColliding = false;
    bool collisionOccurredThisUpdate = false; //used to handle just one collision per update
    public RuntimeAnimatorController[] animations; //animations from east at index 0, going CCW to SE at index 7

    List<Collision2D> collisionsThisUpdate;

    public int clearTrimAccessCount = 0;

    //For collision handling
    BoxCollider2D thisBoxCollider2D;
    Rigidbody2D thisRigidbody2D;

    // Use this for initialization
    void Start () {
        thisBoxCollider2D = GetComponent<BoxCollider2D>();
        thisRigidbody2D = GetComponent<Rigidbody2D>();
        prevMovePosition = transform.position;
        collisionsThisUpdate = new List<Collision2D>();
        collisionsThisUpdate.Clear();
        collisionsThisUpdate.TrimExcess();
        Physics2D.autoSyncTransforms = false; //disable with noting of collisions and such
    }
    // Update is called once per frame
    /*
     noting of collision handling with Update() rather than FixedUpdate(), with maybe implementing my own collision handling with the Player with each of the different objects if doing such - or, could: 
        -maybe double the movement though where movement would still be at the same framerate as that with the reduced timescale for FixedUpdate()
        -somehow handle slowing time and/or such without timeScale being different
         */
    void FixedUpdate() //done in order to come before Update, as a way to handle collision first, and such ... though noting how this would be affected by timeScale
    {
        if (WaveManager.fadeScreenIsActive || PlayerHealth.playerIsDead) //disable TouchMove when shop is active / player is dead
        {
            return;
        }

        prevMovePosition = transform.position;

        var particle = particleGameObj; //must be initialized ... although the original code didn't initialize it?
        foreach (var touch in Input.touches)
        {
            float touchPos_z = Camera.main.transform.position.z;
            Vector3 touchPos_WorldCoords = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, -touchPos_z));
        }
        float moveDir = MoveInnerInput.dir_angleRad;
        if (moveDir != -100f && !isColliding && PlayerWeapons.controllable)
        {
            //check if collision with obstacle
            //move, check collision, if not colliding then move as close to the object as possible without going into the object ... with ray tracing? Or such ...

            transform.position += new Vector3(0.1f * Mathf.Cos(moveDir), 0.1f * Mathf.Sin(moveDir), 0);

            FireShot.laserFollowupShot = false;

            //set animation
            //TODO: Could prevent movement to out of bounds area
        }


        if (!isColliding && PlayerWeapons.controllable) //disallow movement until colliding is over; also, disallow movement HERE ONLY as of 2/21/18, 11:0x-4xPM - noting where movement would be disallowed in MoveInnerInput as handled separately for this - during when PsiLeap would be currently channeled
        {
            //debug movement with arrow keys - although without moveDir
            if (Input.GetKey("w"))
            {
                transform.position += new Vector3(0, 0.1f, 0);
            }
            if (Input.GetKey("s"))
            {
                transform.position += new Vector3(0, -0.1f, 0);
            }
            if (Input.GetKey("a"))
            {
                transform.position += new Vector3(-0.1f, 0, 0);
            }
            if (Input.GetKey("d"))
            {
                transform.position += new Vector3(0.1f, 0, 0);
            }
            if( Input.GetKey("w") || Input.GetKey("s") || Input.GetKey("a") || Input.GetKey("d"))
                FireShot.laserFollowupShot = false;
        }
        if (transform.position != prevMovePosition && !isColliding)
            prevMoveDelta = transform.position - prevMovePosition;

        collisionOccurredThisUpdate = false;
        //if (clearTrimAccessCount++ < 600)
        //{
            collisionsThisUpdate.Clear();
            collisionsThisUpdate.TrimExcess();
        //}

        GameObject ground = GameObject.FindGameObjectWithTag("LevelArea");
        Vector3 levelMin = ground.GetComponent<BoxCollider2D>().bounds.min;
        Vector3 levelMax = ground.GetComponent<BoxCollider2D>().bounds.max;
        //if out of bounds (based on the ground area) by 15, then delete the bullet
        //bool collideWithOutsideLevelArea = transform.position.x < levelMin.x || transform.position.y < levelMin.y || transform.position.x > levelMax.x || transform.position.y > levelMax.y;
        //while (transform.position.x < levelMin.x || transform.position.y < levelMin.y || transform.position.x > levelMax.x || transform.position.y > levelMax.y) //could infinite loop if prevMoveDelta would be Vector3.zero and player would have gotten pushed outside of the level by enemies
        //{
        //    //if (!collisionOccurredThisUpdate)
        //    //{
        //        transform.position -= prevMoveDelta;
        //        //collisionOccurredThisUpdate = true;
        //    //}
        //}
    }



    //issue - detecting collision on the frame after a movement would be done already ... rather than the same frame of movement ... well, used FixedUpdate in order for the reverted movement to occur on the same frame
    //Note regarding the below implementation - would lead to lack of movement 'along' obstacles and/or such with eg. sliding and\or such against walls that could be done in other games
    //and could maybe 'test' both collisions? With maybe objects that would be created and\or such to confirm an object's collision ... before moving in such a direction
    //Noting such an inclusion into the game as TODO

    //Noting of - as of 9/17/18 - what could be a way to not use Rigidbodies for collision handling of the player with doing checking of intersection with each Obstacle object and the player's 'projected collider' for the movement against an obstacle

    void OnCollisionEnter2D(Collision2D c) //didn't realize the "2D" part of the name ... in doing this ... and such regarding collision ...
    {
        if (c.gameObject.CompareTag("Obstacle") && /*!collisionOccurredThisUpdate*/!collisionsThisUpdate.Contains(c))
        {
            collisionsThisUpdate.Add(c);

            //test if no more intersection when reversing just x
            thisBoxCollider2D.offset -= new Vector2(prevMoveDelta.x, 0); //set offset - with corresponding reversed position - for testing
            if (!thisBoxCollider2D.bounds.Intersects(c.collider.bounds))
            {
                thisBoxCollider2D.offset += new Vector2(prevMoveDelta.x, 0); //revert offset
                transform.position -= new Vector3(prevMoveDelta.x, 0, 0);/* //move Rigidbody instead*/
                return;
            }
            else
            {
                thisBoxCollider2D.offset += new Vector2(prevMoveDelta.x, 0); //revert offset
            }
            //thisRigidbody2D.MovePosition(thisRigidbody2D.position + new Vector2(prevMoveDelta.x, 0));

            //test if no more collision\intersection when reversing just y
            thisBoxCollider2D.offset -= new Vector2(0, prevMoveDelta.y); //set offset - with corresponding reversed position - for testing
            if (!thisBoxCollider2D.bounds.Intersects(c.collider.bounds))
            {
                thisBoxCollider2D.offset += new Vector2(0, prevMoveDelta.y); //revert offset
                transform.position -= new Vector3(0, prevMoveDelta.y, 0); //move Rigidbody instead
                return;
            }

            //x and y reversals alone lead to collision; reverse both
            thisBoxCollider2D.offset -= new Vector2(prevMoveDelta.x, 0);

            //if still colliding despite all reversal of thisBoxCollider2D, set this to true
            if (thisBoxCollider2D.bounds.Intersects(c.collider.bounds))
            {
                isColliding = true;
            }
            thisBoxCollider2D.offset += new Vector2(prevMoveDelta.x, prevMoveDelta.y);
            transform.position -= new Vector3(prevMoveDelta.x, prevMoveDelta.y, 0);
        }
    }
    void OnCollisionStay2D(Collision2D c)
    {
        if (c.gameObject.CompareTag("Obstacle") && /*!collisionOccurredThisUpdate*/!collisionsThisUpdate.Contains(c))
        {
            collisionsThisUpdate.Add(c);

            //test if no more intersection when reversing just x
            thisBoxCollider2D.offset -= new Vector2(prevMoveDelta.x, 0); //set offset - with corresponding reversed position - for testing
            if (!thisBoxCollider2D.bounds.Intersects(c.collider.bounds))
            {
                thisBoxCollider2D.offset += new Vector2(prevMoveDelta.x, 0); //revert offset
                transform.position -= new Vector3(prevMoveDelta.x, 0, 0);/* //move Rigidbody instead*/
                return;
            }
            else
            {
                thisBoxCollider2D.offset += new Vector2(prevMoveDelta.x, 0); //revert offset
            }

            //test if no more collision\intersection when reversing just y
            thisBoxCollider2D.offset -= new Vector2(0, prevMoveDelta.y); //set offset - with corresponding reversed position - for testing
            if (!thisBoxCollider2D.bounds.Intersects(c.collider.bounds))
            {
                thisBoxCollider2D.offset += new Vector2(0, prevMoveDelta.y); //revert offset
                //Debug.Log("Reversing y only");
                transform.position -= new Vector3(0, prevMoveDelta.y, 0); //move Rigidbody instead
                return;
            }

            //x and y reversals alone lead to collision; reverse both
            thisBoxCollider2D.offset -= new Vector2(prevMoveDelta.x, 0);

            //if still colliding despite all reversal of thisBoxCollider2D, set this to true
            if (thisBoxCollider2D.bounds.Intersects(c.collider.bounds))
            {
                isColliding = true;
            }
            thisBoxCollider2D.offset += new Vector2(prevMoveDelta.x, prevMoveDelta.y);
            transform.position -= new Vector3(prevMoveDelta.x, prevMoveDelta.y, 0);
        }
    }
    void OnCollisionExit2D(Collision2D c)
    {
        if (c.gameObject.CompareTag("Obstacle"))
        {
            isColliding = false; //allow movement once collision is exited
        }
    }

}
