using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to set the target in AIDestinationSetter for Pursue behaviour in this GameObject. This does so through Update()...with handling FixedUpdate() in certain cases if such would apply.
/// </summary>
public class LocateSeekPursueTarget : MonoBehaviour {

    public Transform finalSeekTarget; //transform of a child in the current GameObject that would be dedicated to being a pursue target.
    Transform pursueTarget;

    public bool pursue; //pursue is enabled

    public bool seekPursueOnProvoked; //pursue or seek only when provoked - may require Wander behaviour
    public bool isProvoked; //enemy is provoked - has only use when coupled with Wander

    public bool newSpeedOnProvoked;
    public int speedOnProvoked;
    public int speedOnNotProvoked;
    public bool newAnimOnProvoked;
        
    public float provokeEnterDistance, provokeExitDistance;

    public float nonWanderAIPathRepathRate;

    //made into these variables in order to lessen FindGameObjectWithTag/FindGameObjectsWithTag calls
    public static GameObject player;
    public static List<GameObject> shadowCharacters;
    public static bool initializedShadowCharacters = false; //made public so that this would be resettable whenever the game would be reset

    // Use this for initialization
    void Start () {
        //default pursue target is player
        player = GameObject.FindGameObjectWithTag("Player");
        if(!initializedShadowCharacters)
        {
            shadowCharacters = new List<GameObject>(GameObject.FindGameObjectsWithTag("ShadowCharacter"));
            initializedShadowCharacters = true;
        }
        GameObject closestTarget = player;
        float closestDistSqr = (player.transform.position - transform.position).sqrMagnitude;
        foreach (GameObject shadowCharacter in shadowCharacters)
        {
            float shadowCharacterDistSqr = (shadowCharacter.transform.position - transform.position).sqrMagnitude;
            if (shadowCharacterDistSqr < closestDistSqr)
            {
                closestTarget = shadowCharacter;
                closestDistSqr = shadowCharacterDistSqr;
            }
        }
        pursueTarget = closestTarget.transform;

        //default seek target is closestTarget
        if(!pursue)
        {
            GetComponent<AIDestinationSetter>().target = closestTarget.transform;
            GetComponent<AILerp>().repathRate = nonWanderAIPathRepathRate;
        }
    }
	
	// Update is called once per frame
	void Update () {
        //simple pursuit - pursuit based on projected position given constant velocity, 
        //and also where the projected position would not necessarily be the point of interception, but just be that at after the duration of (how long the initial pursuer position travelling to the initial target position would take) would have elapsed
        
        //get the closest of the player and Shadows
        GameObject closestTarget = player;
        float closestDistSqr = (player.transform.position - transform.position).sqrMagnitude;
        foreach(GameObject shadowCharacter in shadowCharacters)
        {
            float shadowCharacterDistSqr = (shadowCharacter.transform.position - transform.position).sqrMagnitude;
            if (shadowCharacterDistSqr < closestDistSqr)
            {
                closestTarget = shadowCharacter;
                closestDistSqr = shadowCharacterDistSqr;
            }
        }
        
        Vector2 closestTargetPos2D = new Vector2(closestTarget.transform.position.x, closestTarget.transform.position.y);
        Vector2 thisPos2D = new Vector2(transform.position.x, transform.position.y);
        float distance2D = (closestTargetPos2D - thisPos2D).magnitude; //dealing with squared magnitude might involve less computation, but it makes little difference overall; things like rendering take much more computation
        AILerp AIPathC = GetComponent<AILerp>();


        if (seekPursueOnProvoked && !isProvoked && distance2D <= provokeEnterDistance) //initiated provoke
        {
            isProvoked = true;
            GetComponent<LocateWanderTarget>().isDisabled = true;
            if (!pursue) //if seek, then do seek; update the seek destination if doing seek
            {
                GetComponent<AIDestinationSetter>().target = closestTarget.transform;
            }
            if (newSpeedOnProvoked)
            {
                AIPathC.speed = speedOnProvoked;
            }
            if (newAnimOnProvoked)
            {
                GetComponent<AnimationToMovement>().animationsToUse = 2;
            }
            AIPathC.repathRate = nonWanderAIPathRepathRate; //when not doing wander (where wander would occur on not being provoked, which would be the only case of wander occurring when this LocateSeekPursueTarget component would be present on the current GameObject), then set repath rate to the corresponding repath rate
        }
        else if (seekPursueOnProvoked && isProvoked && distance2D > provokeExitDistance) //end provoke
        {
            isProvoked = false;
            GetComponent<LocateWanderTarget>().isDisabled = false;
            //Debug.Log("Enemy no longer provoked");
            if(newSpeedOnProvoked)
            {
                GetComponent<AILerp>().speed = speedOnNotProvoked;
            }
            if(newAnimOnProvoked)
            {
                GetComponent<AnimationToMovement>().animationsToUse = 1;
            }
            //where repathRate of the AILerp component would be updated by LocateWanderTarget
        }

        //noting that the pursue target would be dependent on the enemy, being specific to them
        if (pursue)
        {
            pursueTarget = closestTarget.transform;
            if (!seekPursueOnProvoked || (seekPursueOnProvoked && isProvoked))
            {
                float distance = (pursueTarget.position - transform.position).magnitude;
                float predictionTime = distance / GetComponent<AILerp>().speed;

                //noting of velocity of pursueTarget and such depending on current target input movement direction, noting of velocity in units per second for target movement - can use prevMoveDelta for this, although such would be based on movement compared to the previous frame - although that would be good enough and rather little in code
                Vector3 closestTargetMoveDelta;
                if (pursueTarget != player.transform)
                {
                    closestTargetMoveDelta = (pursueTarget.position != pursueTarget.gameObject.GetComponent<ShadowPM>().prevMovePosition) ? pursueTarget.gameObject.GetComponent<ShadowPM>().prevMoveDelta : Vector3.zero;
                }
                else
                {
                    closestTargetMoveDelta = (pursueTarget.position != pursueTarget.gameObject.GetComponent<TouchMove>().prevMovePosition) ? pursueTarget.gameObject.GetComponent<TouchMove>().prevMoveDelta : Vector3.zero;
                }
                Vector3 seekTargetPos = pursueTarget.position + (closestTargetMoveDelta * 60) * predictionTime; //the final position to seek, after pursue
                finalSeekTarget.position = seekTargetPos;
                //seek seekTargetPos
                GetComponent<AIDestinationSetter>().target = finalSeekTarget;
            }
        }
        else //if seek
        {
            //set closest target as an update
            if (!seekPursueOnProvoked || (seekPursueOnProvoked && isProvoked))
            {
                GetComponent<AIDestinationSetter>().target = closestTarget.transform;
            }
        }

    }
}
