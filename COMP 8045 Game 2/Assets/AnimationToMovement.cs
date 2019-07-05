using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationToMovement : MonoBehaviour {

    public RuntimeAnimatorController[] animations; //animations from east at index 0, going CCW to SE at index 7
    public RuntimeAnimatorController[] animations2; //second set of animations if used - such as for running as opposed to walking
    public int animationsToUse;
    public int currAnimIndex; //used in order to only set GetComponentInChildren<Animator>().runtimeAnimatorController to a value when it would be a different value

    public Vector2 lastNonZeroVelocity2D = Vector3.zero;
    /// <summary>
    /// Sets animation to be the closest sprite corresponding to the current movement direction.
    /// </summary>
    // Use this for initialization
    void Start () {
        animationsToUse = 1; //default as 1
	}
	
	// Update is called once per frame
	void Update () {
        //UnityEngine.Profiling.Profiler.BeginSample("nonzero code"); //maybe ~0.1-0.4% or so with GetComponent<IAstarAI>().velocity, and noting of maybe up to ~0.7% as I would have seen with GetComponent<AILerp>().velocity2D in ~2-3 or so test sessions of the game as I would have seen, in such profiling
        Vector2 lerpVelocity2D = GetComponent<IAstarAI>().velocity;
        if (lerpVelocity2D != Vector2.zero)
            lastNonZeroVelocity2D = lerpVelocity2D;
        //UnityEngine.Profiling.Profiler.EndSample();

        //get the 45-degree arc around an angle that would be a multiple of 45 that a movement direction would be within, 
        //where the arc would be with the lower bound as inclusive and upper bound as exclusive, where CCW would be positive        
        float movementAngle = ((Mathf.Atan2(lastNonZeroVelocity2D.y, lastNonZeroVelocity2D.x) * 360 / (2 * Mathf.PI))+360)%360;

        //offset all angles and arcs by +45/2 degrees in just this internal computation to make the numbers simpler in the if statements, so east would be at 45/2 degrees with the offset, etc.
        //float movAng_offseted = movementAngle + 45f / 2;
        //update animation to be that for what would be within the corresponding arc

        //cover special case, the initial case, where the direction wraps around
        if (((movementAngle >= 360 - 45f / 2 && movementAngle < 360) || movementAngle < 45f / 2))
        {
            if (currAnimIndex != 0)
            {
                currAnimIndex = 0;
                switch (animationsToUse)
                {
                    case 1:
                        GetComponentInChildren<Animator>().runtimeAnimatorController = animations[currAnimIndex];
                        break;
                    case 2:
                        GetComponentInChildren<Animator>().runtimeAnimatorController = animations2[currAnimIndex];
                        break;
                    default: break;
                }
            }
        }
        else
        {
            int index = (int)((movementAngle + 45f / 2) / 45); //quantize every 45 degrees, starting with -45f/2 inclusive to 45f/2 exclusive as index 0, into an integer that would be the index
            
            if (currAnimIndex != index)
            {
                currAnimIndex = index;
                switch (animationsToUse)
                {
                    case 1:
                        try
                        {
                            GetComponentInChildren<Animator>().runtimeAnimatorController = animations[currAnimIndex]; //12/5/18: Got an error here regarding the index outside bounds of the array; could be maybe a result of this running while the enemies would be destroyed at some different time such that this would run while the animations would be destroyed?
                        }
                        catch(System.Exception e)
                        {
                            Debug.Log("currAnimIndex on out of bounds error: " + currAnimIndex+"; movementAngle on error: "+movementAngle);
                        }
                        break;
                    case 2:
                        GetComponentInChildren<Animator>().runtimeAnimatorController = animations2[currAnimIndex];
                        break;
                    default: break;
                }
            }
        }
    }
}
