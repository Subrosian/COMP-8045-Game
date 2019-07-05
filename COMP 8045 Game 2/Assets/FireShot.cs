using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class FireShot : MonoBehaviour/*, IPointerEnterHandler
     , IPointerExitHandler*/
{

    //Separate isFiring behaviour variable based on if it would be by a player or Shadow, so that whether the player is firing can be accessed via the static variable playerIsFiring for convenient referencing as FireShot.playerIsFiring
    public bool isFiring {
        set
        {
            if(forPlayer)
            {
                playerIsFiring = value;
            }
            else
            {
                thisShadowIsFiring = value;
            }
        }
        get
        {
            if(forPlayer)
            {
                return playerIsFiring;
            }
            else
            {
                return thisShadowIsFiring;
            }
        }
    }
    private bool thisShadowIsFiring;
    public static bool playerIsFiring;
    public static bool laserFollowupShot = false; //may be modifiable by MoveInnerInput - movements leading to maybe ceasing shots to be followup shots
    private float shotTimer;
    public static float shotCooldown; //may depend on the current weapon; <wtl: set><rmth: selected> in PlayerWeapons class; used for player's shot cooldown
    public float shadowShotCooldown; //separate cooldown for Shadows
    //Object re
    private int shotSortingOrder;
    
    Ray ray;
    RaycastHit hit;

    public AudioSource FireShotSoundSource;
    public AudioClip PistolSound;

    //Credit goes to Gumichan01 for the original Laser gun sound (which was edited into 3 separate sound parts)
    public AudioClip LaserGunSoundStart, LaserGunSoundLoopMid, LaserGunSoundEnd;

    float LGSoundTimeLeft; //current amount of remaining time left that the laser gun sound would be to play

    public AudioClip CrossbowSound;
    public AudioClip ShotReleaseDebugSound_ForOnPointerExit;

    public bool forPlayer;
    public int shadowWeaponState; //TODO: could also randomly cycle through weapons as well, as a hard-coded, non-learned aspect of the Shadow AI; could note also of eg. laserFollowupShot if doing such as well

    public const float SHOTCOOLDOWN_DEFAULTPISTOL = 0.18f; //made into a const member because of multiple uses of this value

    ShadowPM PM;
    private RuntimeAnimatorController[] shadowAnimations;

    // Use this for initialization
    void Start () {
        shotTimer = 0;
        if (!forPlayer)
        {
            shadowShotCooldown = SHOTCOOLDOWN_DEFAULTPISTOL;
        }
        LGSoundTimeLeft = 0f;

        shadowWeaponState = 0;
        if (!forPlayer)
        {
            PM = GetComponent<ShadowPM>();
            shadowAnimations = GameObject.FindGameObjectWithTag("Player").GetComponent<TouchMove>().animations;
        }
    }
    public void FireShotEnter()
    {
        Debug.Log("entered");
        shotTimer = Mathf.Min(shotTimer, shotCooldown); //to not be used by a Shadow, so noting: lack of use of a separate shot cooldown
        //isFiring = true;
        shotSortingOrder = 9;
    }    

    // Update is called once per frame
    void Update()
    {
        if (WaveManager.fadeScreenIsActive || PlayerHealth.playerIsDead) //disable while shop is active / player is dead
        {
            return;
        }
        
        bool TIActive = PlayerWeapons.CurrSkill == PlayerWeapons.TEMPORALINHIBITION;
        if (TIActive && forPlayer) {
            shotTimer += Time.deltaTime / Time.timeScale; //adjusted according to Temporal Inhibition
        }
        else
              shotTimer += Time.deltaTime;
        if (shotTimer > 50000) //preventing overflow that could exist of shotTimer
        {
            shotTimer = 100;
        }
        bool usedKey = false;
        //debug firing behaviour
        if (forPlayer)
        {
            if (Input.GetKey("i") || Input.GetKey("j") || Input.GetKey("k") || Input.GetKey("l"))
            {
                if (!isFiring)
                {
                    shotTimer = Mathf.Min(shotTimer, shotCooldown); //as with the thumbstick and preventing cooldown being exceeded in the value of shotTimer that would be used for the next pressing of shot buttons and\or a shot button //with noting of laser and stuttering of said laser, and whatever would be any of the allotted time for said shotTimer having been cut in its overflow of shotCooldown and such leading to a lower rate of such laser shots, as I would note; though using shotCooldown without any multiplier on 2/6/19
                }
                isFiring = true;
                usedKey = true;
            }
        }
        else
        {
            //if a nearest enemy exists, then set isFiring to true
            if(PM.minEnemyToShootRelativeToShadow_prevOrNot != gameObject && PM.isUsing && PM.isFiringThisUpdate && !(GetComponent<ShadowHealth>().isDead))
            {
                isFiring = true;
            }
            else
            {
                isFiring = false;
            }
        }
        float thisShotCooldown;
        if(forPlayer)
        {
            thisShotCooldown = shotCooldown;
        }
        else
        {
            thisShotCooldown = shadowShotCooldown;
        }
        while (isFiring && shotTimer >= thisShotCooldown)
        {
            
            //prevent gun from shooting if without ammo
            int weaponState = PlayerPrefs.GetInt("weaponState");
            if(!forPlayer) //for Shadow AI
            {
                weaponState = shadowWeaponState;
            }
            if (weaponState != 0)
            { //if not the starting gun, which would be the only gun with infinite ammo as of 8/17/18
                int gunAmmo = PlayerPrefs.GetInt("gun" + weaponState + "Ammo"); //get ammo of gun
                if (gunAmmo <= 0)
                {
                    //no ammo for the gun to shoot; prevent shooting here
                    return;
                }
                else
                {
                    //has ammo, so shooting; decrement ammo on shooting
                    gunAmmo--;
                    PlayerPrefs.SetInt("gun" + weaponState + "Ammo", gunAmmo);
                }
            }

            //TBD as of 10/23/17, 11:58PM
            //make bullet at the respective direction, moving at the respective direction, with the respective sprite for that direction if there is no remaining elapsed time before the next shot

            int thisAngle, thisAngleModded;
            GameObject CurrPlayerBullet;
            if (forPlayer)
            {
                FireShotSoundSource.volume = MusicSFXVolChange.SoundVol; //default volume for shot, multiplied by lower valuess
            }
            else
            {
                FireShotSoundSource.volume = MusicSFXVolChange.SoundVol * 0;
            }
            //use moving angle if moving, and shooting angle if shooting
            if (forPlayer)
            {
                if (!isFiring)
                {
                    thisAngle = MoveInnerInput.dir_lastFacedAngleDeg; //noting how angleDeg returns to -100 as default ...
                }
                else
                {
                    thisAngle = ShotInnerInput.dir_lastFacedAngleDeg;
                }
            }
            else //Shadow AI behaviour - set angle to that towards the nearest enemy as a hard-coding, if shooting
            {
                thisAngle = Mathf.RoundToInt(Mathf.Atan2(/*nearest enemy pos*/PM.minEnemyToShootRelativeToShadow_prevOrNot.transform.position.y - transform.position.y, PM.minEnemyToShootRelativeToShadow_prevOrNot.transform.position.x - transform.position.x) * (180 / Mathf.PI) / 45) * 45; //get angle of Shadow towards the nearest enemy (rounded to the nearest multiple of 45 basically) //noting having gotten a missing reference exception on this line on 11/27/18 at some point in said 11/27/18
                thisAngle = (thisAngle + 360) % 360;

                #region ShotInnerInput-based code / ShadowPM animation-based code
                int i_selected = thisAngle / 45;
                Animator animator = GetComponentInChildren<Animator>();
                if (animator.runtimeAnimatorController != shadowAnimations[i_selected]) //making this condition with noting of how some initialize method for the animator would be being called in this getData method
                {
                    GetComponentInChildren<Animator>().runtimeAnimatorController = shadowAnimations[i_selected]; //set animation for the Shadow AI
                }
                #endregion
            }

            //debug firing behaviour
            if (forPlayer)
            {
                if (usedKey)
                {
                    //get average angle of (ie. corresponding to) the keys pressed
                    thisAngle = 0;
                    int thisAngle_keyCount = 0;
                    if (Input.GetKey("i"))
                    {
                        thisAngle += 90;
                        thisAngle_keyCount++;
                    }
                    if (Input.GetKey("j"))
                    {
                        thisAngle += 180;
                        thisAngle_keyCount++;
                    }
                    if (Input.GetKey("k"))
                    {
                        thisAngle += 270;
                        thisAngle_keyCount++;
                    }
                    if (Input.GetKey("l"))
                    {
                        thisAngle += 0;
                        thisAngle_keyCount++;
                    }
                    if (Input.GetKey("k") && Input.GetKey("l"))
                    {
                        //make it as if the "l" would be an angle of 360 in this case
                        thisAngle += 360;
                    }
                    thisAngle /= thisAngle_keyCount;
                    thisAngle = thisAngle % 360;
                }
            }

            int characterWeaponState;
            if(forPlayer)
            {
                characterWeaponState = PlayerWeapons.weaponState;
            }
            else
            {
                characterWeaponState = shadowWeaponState; //could cycle, as mentioned above, with different weapons
            }
            Vector3 shotStartPos;
            if (forPlayer)
            {
                shotStartPos = GameObject.FindGameObjectWithTag("Player").transform.position;
            }
            else
            {
                shotStartPos = transform.position;
            }
            switch (characterWeaponState) {
                case 0:
                    
                    CurrPlayerBullet = Instantiate(Resources.Load("Gun1 Individual Bullets/BulletFab"), shotStartPos + vecFromAngle(0.8f, angleRad(thisAngle)), Quaternion.identity) as GameObject; //at respective direction
                    CurrPlayerBullet.GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>("Gun1 Individual Bullets/Bullet" + thisAngle); //set sprite according to angle
                    CurrPlayerBullet.GetComponent<Rigidbody2D>().velocity = vecFromAngle(TIActive?(12/Time.timeScale):12, angleRad(thisAngle)); //set velocity via adding force
                    CurrPlayerBullet.GetComponent<PlayerBullet>().bulletDmg = 30;
                    
                    FireShotSoundSource.clip = PistolSound;
                    FireShotSoundSource.PlayOneShot(PistolSound);
                    break;
                case 1: //laser gun


                    Vector3 bulletVelocity = vecFromAngle(TIActive ? (12 / Time.timeScale) : 12, angleRad(thisAngle));
                    Vector3 positionOffset = bulletVelocity * (shotTimer - thisShotCooldown);

                    CurrPlayerBullet = Instantiate(Resources.Load("Gun1 Individual Bullets/BulletFab"), shotStartPos + vecFromAngle(0.8f, angleRad(thisAngle)) + positionOffset, Quaternion.AngleAxis(thisAngle-90, new Vector3(0, 0, 90))) as GameObject; //at respective direction, and in position corresponding to what it would be at the time that such would have been created at the last timepoint at the end of the last cooldown as of the current timepoint of the current frame

                    //if immediate follow-up to a prior shot where the shooting action would have not been yet released (since said prior shot), then setting sprite to tail
                    if (!laserFollowupShot)
                    {
                        CurrPlayerBullet.GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>("LaserShot");
                    }
                    else
                    {
                        CurrPlayerBullet.GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>("LaserShot_tail");
                    }
                    CurrPlayerBullet.GetComponentInChildren<SpriteRenderer>().sortingOrder = shotSortingOrder++;
                    if (shotSortingOrder == 100)
                        shotSortingOrder = 9;
                    
                    CurrPlayerBullet.GetComponent<Rigidbody2D>().velocity = bulletVelocity; //set velocity via adding force

                    CurrPlayerBullet.GetComponent<PlayerBullet>().bulletDmg = 20;
                    laserFollowupShot = true;
                    
                    LaserGunSound();

                    break;
                case 2: //crossbow

                    thisAngleModded = ((thisAngle - 45) + 360)%360;
                    Color teal = new Color(0.5f, 1, 0.8f);

                    CurrPlayerBullet = Instantiate(Resources.Load("Gun1 Individual Bullets/BulletFab"), shotStartPos + vecFromAngle(0.8f, angleRad(thisAngleModded)), Quaternion.identity) as GameObject; //at respective direction
                    CurrPlayerBullet.GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>("Gun1 Individual Bullets/Bullet" + thisAngleModded); //set sprite according to angle
                    CurrPlayerBullet.GetComponentInChildren<SpriteRenderer>().color = teal;
                    CurrPlayerBullet.GetComponent<Rigidbody2D>().velocity = vecFromAngle(TIActive ? (12 / Time.timeScale) : 12, angleRad(thisAngleModded)); //set velocity via adding force
                    CurrPlayerBullet.GetComponent<PlayerBullet>().bulletDmg = 30;

                    thisAngleModded = thisAngle % 360;
                    CurrPlayerBullet = Instantiate(Resources.Load("Gun1 Individual Bullets/BulletFab"), shotStartPos + vecFromAngle(0.8f, angleRad(thisAngleModded)), Quaternion.identity) as GameObject; //at respective direction
                    CurrPlayerBullet.GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>("Gun1 Individual Bullets/Bullet" + thisAngleModded); //set sprite according to angle
                    CurrPlayerBullet.GetComponentInChildren<SpriteRenderer>().color = teal;
                    CurrPlayerBullet.GetComponent<Rigidbody2D>().velocity = vecFromAngle(TIActive ? (12 / Time.timeScale) : 12, angleRad(thisAngleModded)); //set velocity via adding force
                    CurrPlayerBullet.GetComponent<PlayerBullet>().bulletDmg = 30;

                    thisAngleModded = (thisAngle + 45) % 360;
                    CurrPlayerBullet = Instantiate(Resources.Load("Gun1 Individual Bullets/BulletFab"), shotStartPos + vecFromAngle(0.8f, angleRad(thisAngleModded)), Quaternion.identity) as GameObject; //at respective direction
                    CurrPlayerBullet.GetComponentInChildren<SpriteRenderer>().sprite = Resources.Load<Sprite>("Gun1 Individual Bullets/Bullet" + thisAngleModded); //set sprite according to angle
                    CurrPlayerBullet.GetComponentInChildren<SpriteRenderer>().color = teal;
                    CurrPlayerBullet.GetComponent<Rigidbody2D>().velocity = vecFromAngle(TIActive ? (12 / Time.timeScale) : 12, angleRad(thisAngleModded)); //set velocity via adding force
                    CurrPlayerBullet.GetComponent<PlayerBullet>().bulletDmg = 30;
                    //TBD: play sound
                    FireShotSoundSource.clip = CrossbowSound;
                    FireShotSoundSource.volume *= 0.4f;
                    FireShotSoundSource.PlayOneShot(CrossbowSound);
                    break;

            }
            
            shotTimer -= thisShotCooldown; //repeat until all shots fired as of the current frame would be in place
        }

        
        //Part of laser gun sound handling
        //prolong laser gun mid sound if there is more sound time left than the remaining 'LoopMid' sound duration, with checking and looping at <=0.1 of remaining time left
        if (LGSoundTimeLeft > 0.1f && FireShotSoundSource.clip == LaserGunSoundLoopMid && LaserGunSoundLoopMid.length - FireShotSoundSource.time <= 0.1f)
        {
            FireShotSoundSource.clip = LaserGunSoundLoopMid;
            FireShotSoundSource.Stop();
            FireShotSoundSource.Play();
            //Debug.Log("Laser gun sound prolong mid sound");
        }
        //start the end sound of a laser if not currently firing a shot and time would be approaching its end such that said time would be better used as the end sound instead, replacing any currently active laser sound
        if (!isFiring && LGSoundTimeLeft > 0/* && FireShotSoundSource.isPlaying*/ && LGSoundTimeLeft <= LaserGunSoundEnd.length && FireShotSoundSource.clip != LaserGunSoundEnd)
        {
            FireShotSoundSource.clip = LaserGunSoundEnd;
            FireShotSoundSource.Stop();
            FireShotSoundSource.Play();
            //Debug.Log("Laser gun sound play end sound");
        }
        LGSoundTimeLeft = Mathf.Max(0f, LGSoundTimeLeft-PlayerWeapons.pDeltaTime);
    }

    
    void LaserGunSound()
    {
        //Part of laser sound handling
        //start sound, or prolong sound at this point if it would be a time to prolong it at this point
        bool canProlongFromStart = (FireShotSoundSource.clip == LaserGunSoundStart) && FireShotSoundSource.time >= LaserGunSoundStart.length - 0.17f;
        if (LGSoundTimeLeft > 0f && (/*canProlongFromEnd || */canProlongFromStart)) //still in the middle of playing; swap to mid if at least enough of the Start clip would have already occurred
        {
            FireShotSoundSource.clip = LaserGunSoundLoopMid;
            FireShotSoundSource.Stop();
            FireShotSoundSource.Play();
            //Debug.Log("Laser gun sound prolong in LaserGunSound");
        }
        else if (LGSoundTimeLeft <= 0f || FireShotSoundSource.clip == LaserGunSoundEnd)
        {
            FireShotSoundSource.clip = LaserGunSoundStart;
            FireShotSoundSource.Stop();
            FireShotSoundSource.Play();
            //Debug.Log("Laser gun sound start");
        }
        LGSoundTimeLeft = Mathf.Min(LGSoundTimeLeft + LaserGunSoundStart.length, 1f); //add sound time, with the max time being 1f

    }

    float angleRad(float angleDeg)
    {
        return angleDeg * 2 * Mathf.PI / 360;
    }
    Vector3 vecFromAngle(float distance, float angleRad)
    {
        return new Vector3(Mathf.Cos(angleRad) * distance, Mathf.Sin(angleRad) * distance/* - forgot distance here*/, 0);
    }
}
