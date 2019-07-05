using UnityEngine;
using System.Collections;

public class PlayerBullet : MonoBehaviour
{

    // Use this for initialization
    GameObject ground;

    //int used to prevent immediate consecutive damage, by only allowing damage after 0.5sec. has elapsed before the next hit.
    public float timeSincePlayerDamage = 0;
    public const float timeBeforeDamagingPlayer = 0.5f; //
    public bool justHitPlayer = false;
    private int numCollisions = 0;
    float playerImmunityTime = 0;
    public int bulletDmg;

    void Start()
    {
        //When the bullets go out of bounds, then destroy them.
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        numCollisions++;

        //Using triggers instead for enemies, so that short enemies can still get hit and bullet ricochets don't repeat for the same bullet repeatedly
        if ((other.gameObject.transform.parent != null && other.gameObject.transform.parent.gameObject.CompareTag("Enemy")) || other.gameObject.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = other.gameObject.GetComponent<EnemyHealth>();
            if (DayNightCycle.isDay)
                enemyHealth.TakeDamage(bulletDmg);
            else
                enemyHealth.TakeDamage(bulletDmg / 2);
            
            //maybe any collision effect of a bullet hitting, maybe setting off some animation...if I would have some effect like that, as something that could be TBD if worthwhile
            
            Destroy(gameObject); //destroy as soon as it does damage
        }
    }
}
