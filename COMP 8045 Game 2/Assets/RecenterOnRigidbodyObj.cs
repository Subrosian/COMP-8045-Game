using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecenterOnRigidbodyObj : MonoBehaviour {

	// Use this for initialization
	void Start () {
        recenterInterval = GetComponent<AILerp>().repathRate * 5;
    }
    float timeSinceRecenter = 0f;
    float recenterInterval = 0f;

    // Update is called once per frame
    void FixedUpdate () {

        if (timeSinceRecenter >= recenterInterval)
        {
            //periodically do this, rather than immediately, in order to avoid updating position relative to the path due to rigidbody movements and thus redirecting towards the path immediately
            //recenter on child obj. in case that the child obj. changes position, such as due to independent child movement due to physics with such interactions with its RigidBody2D, when path would be started
            Transform rigidBodyChildTransform = GetComponentInChildren<Rigidbody2D>().transform;
            if (transform.position != rigidBodyChildTransform.position)
            {
                GetComponent<AILerp>().Teleport(rigidBodyChildTransform.position, false);
                rigidBodyChildTransform.localPosition = Vector3.zero;
            }
            timeSinceRecenter = 0f;
        }
        timeSinceRecenter += Time.deltaTime;
    }
}
