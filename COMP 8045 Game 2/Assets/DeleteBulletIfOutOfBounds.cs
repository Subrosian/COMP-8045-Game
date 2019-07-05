using UnityEngine;
using System;

public class DeleteBulletIfOutOfBounds : MonoBehaviour {

    void Awake()
    {

    }

	// Use this for initialization
	void Start () {
	    
	}
    
    // Update is called once per frame
    void Update () {
    }

    float angleRad(float angleDeg)
    {
        return angleDeg * 2 * Mathf.PI / 360;
    }
    Vector3 vecFromAngle(float distance, float angleRad)
    {
        return new Vector3(Mathf.Cos(angleRad) * distance, Mathf.Sin(angleRad), 0);
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        //remove any bullets that leave the trigger of a GameObject with this script (which would be to be Ground)
        if(other.CompareTag("Bullet"))
        {
            Destroy(other.gameObject);
        }
    }
}