using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocateWanderTarget : MonoBehaviour {

    public float wanderOffset;
    public float wanderRadius;
    public float wanderRate; //the range at which the wander orientation can change? Well, the 'extent' in one of the directions at which the wander orientation can change - in the circle that would be in front of the agent that the target would lie on... so the circle would still use the angle of the agent as the 'base' despite the agent being in a different position ... with such a range being per each update...

    public Transform finalSeekTarget;

    float wanderOrientation;
    AILerp ai;

    public bool isDisabled;

	// Use this for initialization
	void Start () {
        ai = GetComponent<AILerp>();
        wanderOrientation = 0;
	}
	
    float randomBinomial()
    {
        return Random.Range(0,1f) - Random.Range(0, 1f);
    }

    /// <summary>
    /// Returns a unit vector at the angle angleRad.
    /// </summary>
    /// <param name="angleRad"></param>
    /// <returns></returns>
    Vector3 toVector(float angleRad)
    {
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0);
    }

    void updateWanderTarget()
    {
        AILerp AIPathC = GetComponent<AILerp>();
        //get position in a circle ahead of the agent, but only at positions that would be not obstacles...well, Pursue would have the same thing happening to it ...
        //Newer: shift the circle to where the arc/circle would be completely outside any obstacle if possible
        //where I could use a CircleCollider2D with any checking of intersection until outside obstacles? Along the arc? Maybe doing some sort of quantized search, with maybe binary searching in between a non-intersecting angle and an intersecting angle to find the angle closest to the forward position that would not be intersecting with an obstacle? To the nearest degree? Though noting of eg. intersecting at 1/2 but not at 3/4 and such ... with non-continuous obstacles ... and noting of
        //eg. checking ends of obstacles, with finding the point that would be a radius away from the enemy and also the end of the obstacle, as a point at the center of such a circle, that would be on the arc on which such a wander circle could be ... basically the intersection of such distances ... and then determining the angle between such points ... and checking such ends ...
        //or as a simpler implementation, basically just making the CircleCollider2D have collision, moving to the nearest non-obstacle position, or shifting its position to inside the boundaries of the stage though keeping the angle the same, where the forward position could change significantly with such a circle and with any wander 'normalizing' (to what would be what one might call 'wander behaviour' then) that way

        //according to the COMP8901 text, also such a circle being with angles within a range of the angle of the agent's current orientation
        wanderOrientation += randomBinomial() * wanderRate;
        
        //rotation being, from facing the bottom, -270 to 90, CCW being positive, as rotated about the x axis; adding 180 degrees would make the angle what <<I would be familiar with><YKWIM>> and lead to the intended wanderTargetPosition vector
        float targetAngle = wanderOrientation + Mathf.Atan2(AIPathC.velocity2D.y, AIPathC.velocity2D.x); //angle of random rotation amount + character rotation //where character rotation would be to make the circle's orientation 'relative' to the character's facing direction
                                                                                                                                                                                                                                                                                                                                                    //noting of how to get character rotation ... whether such would be in AIPath or somewhere else
                                                                                                                                                                                                                                                                                                                                                    //well, it being in transforms ...but the character still rotating in its entirety while the rotation in Unity would just be displaying such around numbers of magnitude of 180 and 90 and\or such
                                                                                                                                                                                                                                                                                                                                                    //noting of such and the resulting target ...
        UnityEngine.Profiling.Profiler.BeginSample("LocateWanderTarget sample");

        Vector3 wanderCirclePos = wanderOffset * toVector(/*transform.rotation.eulerAngles.x * Mathf.PI * 2 / 360 + Mathf.PI*/Mathf.Atan2(AIPathC.velocity2D.y, AIPathC.velocity2D.x)) + transform.position; //position of center of wander circle
        //if outside stage, shift circlePos to within stage

        //stage top, bottom, left, and right that would be in the positions of the manually placed objects rather than in the automatically placed objects from RepeatedTiling, which would work as well
        //float StageTop = GameObject.Find("TopLeftTile").transform.position.y;
        //float StageBottom = GameObject.Find("BottomRightTile").transform.position.y;
        //float StageLeft = GameObject.Find("TopLeftTile").transform.position.x;
        //float StageRight = GameObject.Find("BottomRightTile").transform.position.x;
        if (wanderCirclePos.y > RepeatedTiling.StageTop - wanderRadius)
        {
            wanderCirclePos.y = RepeatedTiling.StageTop - wanderRadius;
        }
        if (wanderCirclePos.x > RepeatedTiling.StageRight - wanderRadius)
        {
            wanderCirclePos.x = RepeatedTiling.StageRight - wanderRadius;
        }
        if (wanderCirclePos.x < RepeatedTiling.StageLeft + wanderRadius)
        {
            wanderCirclePos.x = RepeatedTiling.StageLeft + wanderRadius;
        }
        if (wanderCirclePos.y < RepeatedTiling.StageBottom + wanderRadius)
        {
            wanderCirclePos.y = RepeatedTiling.StageBottom + wanderRadius;
        }

        //position of point on circle at targetOrientation angle, where said circle would be with its center at wanderOffset from the forward direction of the character relative to the character
        Vector3 wanderTargetPosition = toVector(targetAngle) * wanderRadius +
                                       wanderCirclePos;

        finalSeekTarget.position = wanderTargetPosition;
        GetComponent<AIDestinationSetter>().target = finalSeekTarget;
        //recalculate path when a new destination is set ...or, have repath rate such that the interval between each repath would be the minimum time to reach the destination set, i.e. in a direct movement towards the destination at max speed - as a somewhat simple way of implementing such
        float distFromDest = (new Vector2(transform.position.x, transform.position.y) - new Vector2(finalSeekTarget.position.x, finalSeekTarget.position.y)).magnitude;
        AIPathC.repathRate = distFromDest / AIPathC.speed;

        UnityEngine.Profiling.Profiler.EndSample();

        //update repath rate of AIPath with s
    }

	// Update is called once per frame
	void Update () {
        if (!isDisabled)
        {
            updateWanderTarget();
        }
    }
}
