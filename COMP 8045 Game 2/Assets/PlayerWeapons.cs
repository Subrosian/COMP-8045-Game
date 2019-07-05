using Pathfinding;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerWeapons : MonoBehaviour {

    public Image PistolImg, LaserGunImg, CrossbowImg, PsiBurstImg, DeltaPrisonImg, PsiLeapImg, TemporalInhibitionImg;
    public Slider skillPtsSlider;

    public static int weaponState = 0;
    public static bool controllable;
    public Vector3 PsiLeap_InitPosition;

    //delta time for the player
    public static float pDeltaTime; //delta time that persists in increasing, even while paused
    public static float pLastrealtimeSinceStartup; //for the prev. Update

    //saved data of whether these guns are available - where gun0 is already available by default
    public static int gun1
    {
        get
        {
            return PlayerPrefs.GetInt("gun1");
        }
        set
        {
            PlayerPrefs.SetInt("gun1", value);
        }
    }
    public static int gun2
    {
        get
        {
            return PlayerPrefs.GetInt("gun2");
        }
        set
        {
            PlayerPrefs.SetInt("gun2", value);
        }
    }

    //ammo for the respective guns (laser, shotgun respectively)
    public static int gun1Ammo { //laser ammo
        get
        {
            return PlayerPrefs.GetInt("gun1Ammo");
        }
        set
        {
            PlayerPrefs.SetInt("gun1Ammo", value);
        }
    }
    public static int gun2Ammo //shotgun ammo
    {
        get
        {
            return PlayerPrefs.GetInt("gun2Ammo");
        }
        set
        {
            PlayerPrefs.SetInt("gun2Ammo", value);
        }
    }

    public const int NONE = 0, PSIBURST = 1, DELTAPRISON = 2, PSILEAP = 3, TEMPORALINHIBITION = 4;
    public static int CurrSkill = NONE; //skill that would be currently in play

    public static int gunAmmoPrice(int gunNum)
    {
        int gunAmmoPrice = 0;
        switch (gunNum)
        {
            case 1: //laser gun
                gunAmmoPrice = 400;
                break;
            case 2: //crossbow
                gunAmmoPrice = 200;
                break;
        }
        return gunAmmoPrice;
    }
    public static int gunAmmoBundleQty(int gunNum)
    {
        int gunAmmoBundleQty = 0;
        switch (gunNum)
        {
            case 1: //laser gun
                gunAmmoBundleQty = 200;
                break;
            case 2: //crossbow
                gunAmmoBundleQty = 100;
                break;
        }
        return gunAmmoBundleQty;
    }
    public static int gunPrice(int gunNum)
    {
        int gunPrice = 0;
        switch (gunNum)
        {
            case 1: //laser gun
                gunPrice = 800;
                break;
            case 2: //crossbow
                gunPrice = 800;
                break;
        }
        return gunPrice;
    }

    public static int sk_Price(int skillNum)
    {
        int skillPrice = 0;
        switch(skillNum)
        {
            //new formula for encouraging same skill use to not occur, i.e. discouraging the same skill purchase and such with how cheap that other skills would be to purchase
            case PSIBURST:
                skillPrice = 400 + 200 * (int)Mathf.Pow(2, sk_PsiBurst-1);
                break;
            case DELTAPRISON:
                skillPrice = 400 + 200 * (int)Mathf.Pow(2, sk_DeltaPrison - 1);
                break;
            case PSILEAP:
                skillPrice = 400 + 200 * (int)Mathf.Pow(2, sk_PsiLeap - 1);
                break;
            case TEMPORALINHIBITION:
                skillPrice = 400 + 200 * (int)Mathf.Pow(2, sk_TemporalInhibition - 1);
                break;
        }
        return skillPrice;
    }

    public static int sk_PsiBurst_Price
    {
        get
        {
            return sk_Price(PSIBURST);
        }
    }
    public static int sk_DeltaPrison_Price
    {
        get
        {
            return sk_Price(DELTAPRISON);
        }
    }
    public static int sk_PsiLeap_Price
    {
        get
        {
            return sk_Price(PSILEAP);
        }
    }
    public static int sk_TemporalInhibition_Price
    {
        get
        {
            return sk_Price(TEMPORALINHIBITION);
        }
    }

    public static Vector3 DeltaPrisonTriangleBaseScale;
    public static Vector3 DeltaPrisonTriangleBlackBaseScale;
    public static Vector2[] DeltaPrisonPolygonColliderBasePoints;

    public static int sk_PsiBurst
    {
        get
        {
            if (PlayerPrefs.HasKey("skill1"))
                return PlayerPrefs.GetInt("skill1");
            else
            {
                return 0;
            }
        }
        set
        {
            PlayerPrefs.SetInt("skill1", value);
        }
    }
    public static int sk_DeltaPrison
    {
        get
        {
            if (PlayerPrefs.HasKey("skill2"))
                return PlayerPrefs.GetInt("skill2");
            else
            {
                return 0;
            }
        }
        set
        {
            PlayerPrefs.SetInt("skill2", value);
            //update corresponding delta prison triangle

            GameObject player = GameObject.FindGameObjectWithTag("Player");

            //scale the triangle collider; one would have to center it, multiply the points' positions, then uncenter it, but it would already start as centered so the centering and uncentering wouldn't be needed ...well, any offset that the collider would have would be 'multiplied' if the triangle would be offset as the delta prison shapes that would be visible in game would be
            //basically, this would be scaling the triangle as the triangle that would be visible in game would be
            Vector2[] newPoints = (Vector2[])(player.GetComponent<PolygonCollider2D>().points).Clone();

            float scaleInc = 0.6f;
            float baseMultiplier = 1.3f;

            float xOffset = player.transform.Find("DeltaPrisonTriangle").localPosition.x;
            float yOffset = player.transform.Find("DeltaPrisonTriangle").localPosition.y;
            
            for (int i=0; i<player.GetComponent<PolygonCollider2D>().points.Length; i++)
            {
                Debug.Log("Point: (" + newPoints[i].x + "," + newPoints[i].y + ")");
                //increase in proportion to that of DeltaPrisonTriangle in relation to its base version
                float xScale = (DeltaPrisonTriangleBaseScale.x * baseMultiplier + scaleInc * (value - 1)) / DeltaPrisonTriangleBaseScale.x;
                newPoints[i].x = (DeltaPrisonPolygonColliderBasePoints[i].x - xOffset) * xScale + xOffset;
                float yScale = (DeltaPrisonTriangleBaseScale.y * baseMultiplier + scaleInc * (value - 1)) / DeltaPrisonTriangleBaseScale.y;
                newPoints[i].y = (DeltaPrisonPolygonColliderBasePoints[i].y - yOffset) * yScale + yOffset;
            }
            player.GetComponent<PolygonCollider2D>().points = newPoints;

            player.transform.Find("DeltaPrisonTriangle").localScale = new Vector3(DeltaPrisonTriangleBaseScale.x * baseMultiplier + scaleInc * (value - 1), DeltaPrisonTriangleBaseScale.y * baseMultiplier + scaleInc * (value - 1), 0);
            player.transform.Find("DeltaPrisonTriangleBlack").localScale = new Vector3(DeltaPrisonTriangleBlackBaseScale.x * baseMultiplier + scaleInc * (value - 1), DeltaPrisonTriangleBlackBaseScale.y * baseMultiplier + scaleInc * (value - 1), 0);
            Debug.Log("Delta Prison Triangle y: "+player.transform.Find("DeltaPrisonTriangle").position.y);
        }
    }
    public static int sk_PsiLeap
    {
        get
        {
            if (PlayerPrefs.HasKey("skill3"))
                return PlayerPrefs.GetInt("skill3");
            else
            {
                return 0;
            }
        }
        set
        {
            PlayerPrefs.SetInt("skill3", value);
        }
    }

    public static int sk_TemporalInhibition
    {
        get
        {
            if (PlayerPrefs.HasKey("skill4"))
                return PlayerPrefs.GetInt("skill4");
            else
            {
                return 0;
            }
        }
        set
        {
            PlayerPrefs.SetInt("skill4", value);
        }
    }

    public static int sk_Level(int skillNum)
    {
        if (PlayerPrefs.HasKey("skill"+skillNum))
            return PlayerPrefs.GetInt("skill"+skillNum);
        else
        {
            return 0;
        }
    }

    private float skillTimer;
    private float DeltaPrisonEffectTimer;


    //Some spell costs being edited, as part of balancing
    public int PsiBurstCost;
    public int DeltaPrisonCost; //ability cost also increasing with respect to level? Or just making this a 'crowd control' ability, with noting of such regarding whether such would have its cost increase or not with higher level as of 2/7/19 and\or on 2/7/19 <<<as I would note><YKWIM>>< - note this is as I understand it to mean>>
    public int PsiLeapCost;
    public int TemporalInhibitionCost;

    public GameObject TemporalInhibitionTransShade;

    public const float MAX_SKILLPTS = 100;
    //public float SkillPts = MAX_SKILLPTS; //hardcoded as 100
    public float SkillPts //property to synchronize currentHealth with the slider
    {
        set
        {
            PlayerPrefs.SetFloat("currentSkillPts", value);
            skillPtsSlider.value = value;
        }
        get
        {
            return PlayerPrefs.GetFloat("currentSkillPts");
        }
    }
    public float skillPtsRegenRate = 2f; //regen per second

    public static int GlobalScore
    {
        get
        {
            if (PlayerPrefs.HasKey("score"))
                return PlayerPrefs.GetInt("score");
            else
            {
                return 0;
            }
        }
        set
        {
            PlayerPrefs.SetInt("score", value);
        }
    }

    public AudioSource SkillSoundSource;
    public AudioClip PsiBurstSound, DeltaPrisonTriangleSound, DeltaPrisonFreezeSound, PsiLeapDisappearSound, PsiLeapReappearSound, TemporalInhibitionStartSound, TemporalInhibitionEndSound, SkillErrorSound;

    private bool selectionInitialization;

    public void InitializeNonShopPeriod() //called on Start() of PlayerWeapons.cs, as well as on continuing after Shop phases
    {
        controllable = true;

        //initialize temporal inhibition translucent shade
        Color TIColor = TemporalInhibitionTransShade.GetComponent<SpriteRenderer>().color;
        TemporalInhibitionTransShade.GetComponent<SpriteRenderer>().color = new Color(TIColor.r, TIColor.g, TIColor.b, TIColor.a);

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        //initialize delta prison triangle
        DeltaPrisonTriangleBaseScale = player.transform.Find("DeltaPrisonTriangle").localScale;
        DeltaPrisonTriangleBlackBaseScale = player.transform.Find("DeltaPrisonTriangleBlack").localScale;
        DeltaPrisonPolygonColliderBasePoints = (Vector2[])(player.GetComponent<PolygonCollider2D>().points).Clone();

        //hide anything that isn't obtained
        if (sk_DeltaPrison == 0)
        {
            DeltaPrisonImg.raycastTarget = false;
        }
        if (sk_PsiBurst == 0)
        {
            PsiBurstImg.raycastTarget = false;
        }
        if (sk_PsiLeap == 0)
        {
            PsiLeapImg.raycastTarget = false;
        }
        if (sk_TemporalInhibition == 0)
        {
            TemporalInhibitionImg.raycastTarget = false;
        }
        if (gun1 == 0)
        {
            LaserGunImg.raycastTarget = false;
            //no label text, unlike in Testosterone Jones
        }
        if (gun2 == 0)
        {
            CrossbowImg.raycastTarget = false;
        }


        if (PlayerPrefs.HasKey("weaponState"))
        {
            weaponState = PlayerPrefs.GetInt("weaponState");
            if ((weaponState == 1 && gun1 == 0) || (weaponState == 2 && gun2 == 0)) //if the player does not have the selected gun (such as in selecting the gun in the shop without said gun being purchased and not having been unselected before starting the select next wave as I would note), then revert gun to gun 0
            {
                Debug.Log("weaponState set to 0");
                weaponState = 0;
            }
            Debug.Log("weaponState: " + weaponState);
            WeaponSelected(weaponState);
        }
        else
        {
            Debug.Log("WeaponSelected(0) with weaponState: " + weaponState);
            WeaponSelected(0); //make the starting gun default, which would make the other guns black on the HUD <<as I would note><YKWIM>>
        }
    }


    void NewGame_Weapons()
    {
        PlayerWeapons.sk_PsiBurst = 0;
        PlayerPrefs.SetInt("skill2", 0); //with avoiding the update/edit to game objects as a result of the skill edit
        PlayerWeapons.sk_PsiLeap = 0;
        PlayerWeapons.sk_TemporalInhibition = 0;
        PlayerWeapons.gun1 = 0;
        PlayerWeapons.gun2 = 0;
        PlayerWeapons.gun1Ammo = 0;
        PlayerWeapons.gun2Ammo = 0;
        PlayerWeapons.weaponState = 0;
        if (PlayerPrefs.HasKey("weaponState"))
        {
            PlayerPrefs.DeleteKey("weaponState");
        }
        PlayerWeapons.GlobalScore = 0;
        SkillPts = MAX_SKILLPTS;
    }

    // Use this for initialization
    void Start ()
    {
        if (WaveManager.isNewGame)
        {
            NewGame_Weapons();
        }
        selectionInitialization = true;
        InitializeNonShopPeriod();

        //for start of the scene, hide all unobtained skills and weapons immediately
        if (sk_DeltaPrison == 0)
        {
            DeltaPrisonImg.GetComponent<CanvasRenderer>().SetAlpha(0);
        }
        if (sk_PsiBurst == 0)
        {
            PsiBurstImg.GetComponent<CanvasRenderer>().SetAlpha(0);
        }
        if (sk_PsiLeap == 0)
        {
            PsiLeapImg.GetComponent<CanvasRenderer>().SetAlpha(0);
        }
        if (sk_TemporalInhibition == 0)
        {
            TemporalInhibitionImg.GetComponent<CanvasRenderer>().SetAlpha(0);
        }
        if (gun1 == 0)
        {
            LaserGunImg.GetComponent<CanvasRenderer>().SetAlpha(0);
            //no label text, unlike in Testosterone Jones
        }
        if (gun2 == 0)
        {
            CrossbowImg.GetComponent<CanvasRenderer>().SetAlpha(0);
        }

        selectionInitialization = false; //end selection initialization after it was done in InitializeNonShopPeriod() <<as I would note><YKWIM>>
        PlayerWeapons.sk_DeltaPrison = PlayerWeapons.sk_DeltaPrison; //re-assigning, due to specifically just this skill property updating game objects; only doing it for this skill property, however

        //some debug - enable all, or not
        if (WaveManager.DEBUG_WEAPONS)
        {
            sk_TemporalInhibition = 3;
            sk_DeltaPrison = 5;
            sk_PsiLeap = 3;
            sk_PsiBurst = 3;
            gun1 = 1;
            gun2 = 1;
            GlobalScore += 5000;
            //debug code - setting ammo
            gun1Ammo = 1000;
            gun2Ammo = 1000;
        }
        //8/16/18 - in fixing an error resulting from FindGameObjectsWithTag leading to finding GameObjects not visible in the Unity scene: used for removing invisible objects from a bug - got fix here: https://answers.unity.com/questions/1012246/findgameobjectswithtag-finds-an-object-that-doesnt.html - and could note memory being near 100% when I was running Unity in such an error occurring
        SkillSoundSource.volume = MusicSFXVolChange.SoundVol;

        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            GameObject enemy_rootParent = enemy;
            while (enemy_rootParent.transform.parent != null)
            {
                enemy_rootParent = enemy_rootParent.transform.parent.gameObject;
            }
            Debug.Log("Enemy: " + enemy_rootParent.transform.name + ", " + enemy.transform.childCount + ", " + enemy.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite+", enemy's EnemyHealth component is not null: "+(enemy_rootParent.GetComponentInChildren<EnemyHealth>() != null)+"; enemy is boss: "+ enemy_rootParent.GetComponentInChildren<EnemyHealth>().isBoss);
            //var objectsWithTag = GameObject.FindGameObjectsWithTag();
            //for (int i = objectsWithTag.Length - 1; i > -1; i--)
            if (enemy_rootParent.GetComponentInChildren<EnemyHealth>() != null)
            {
                if (!enemy_rootParent.GetComponentInChildren<EnemyHealth>().isBoss) //prevent bosses accidentally getting destroyed if they would spawn and get destroyed
                {
                    Destroy(enemy_rootParent);
                }
            }
            else
            {
                Destroy(enemy_rootParent);
            }
        }
    }

    public void WeaponSelected(int gunNum) //to not be used by a Shadow, so noting: lack of use of a separate shot cooldown
    {
        if(!selectionInitialization && (WaveManager.fadeScreenIsActive || PlayerHealth.playerIsDead)) //only allow selection (and corresponding setting of weapon image colors) while shop would not be active and player would not be dead (preventing setting of alphas of the weapon images' colors, with any of fading involved in shop and such
        {
            return;
        }
        //Debug.Log("WeaponSelected called?"); //for debugging of LaserGunImg and CrossBowImg colors
        weaponState = gunNum;

        //set weapon select, and cooldown here
        switch(weaponState) //noting of Shop.colorWithCurrAlpha and such not being necessary for PistolImg.color if PistolImg.color would not ever be with alpha set to 0
        {
            case 0:
                PistolImg.color = Shop.colorWithCurrAlpha(PistolImg.color, Color.white);
                LaserGunImg.color = Shop.colorWithCurrAlpha(LaserGunImg.color, Color.black);
                CrossbowImg.color = Shop.colorWithCurrAlpha(CrossbowImg.color, Color.black);

                FireShot.shotCooldown = FireShot.SHOTCOOLDOWN_DEFAULTPISTOL;
                break;
            case 1:
                PistolImg.color = Shop.colorWithCurrAlpha(PistolImg.color, Color.black);
                LaserGunImg.color = Shop.colorWithCurrAlpha(LaserGunImg.color, Color.white);
                CrossbowImg.color = Shop.colorWithCurrAlpha(CrossbowImg.color, Color.black);

                FireShot.shotCooldown = 0.04f;
                break;
            case 2:
                PistolImg.color = Shop.colorWithCurrAlpha(PistolImg.color, Color.black);
                LaserGunImg.color = Shop.colorWithCurrAlpha(LaserGunImg.color, Color.black);
                CrossbowImg.color = Shop.colorWithCurrAlpha(CrossbowImg.color, Color.white);

                FireShot.shotCooldown = 0.3f;
                break;
            default:
                break;
        }
        //play a sound in indicating weapon switch here

        //save weapon choice as weaponState, which can be loaded e.g. at the start of the next initialization of the gameplay scene
        Debug.Log("shotCooldown: " + FireShot.shotCooldown);
        PlayerPrefs.SetInt("weaponState", weaponState);
    }

    public void updateDeltaTime()
    {
        pDeltaTime = Time.realtimeSinceStartup - pLastrealtimeSinceStartup;
        pLastrealtimeSinceStartup = Time.realtimeSinceStartup;
    }
    public void SkillSelected(int skSelected)
    {
        //select skills, set as active, update skill points - if with enough skill points
        //TBD as of 2/19/18, 7:40PM< - if not 7:41PM>: <<complete what would be in the code underlined by red with what would be intended><YKWIM>>

        //disallow use of skills if player would be dead
        if (PlayerHealth.playerIsDead)
        {
            return;
        }

        //Psi Burst input
        if (sk_PsiBurst != 0 && skSelected == PSIBURST)
        {
            if (SkillPts >= PsiBurstCost && CurrSkill == NONE)
            {
                CurrSkill = PSIBURST;
                SkillPts -= PsiBurstCost;
            }
            else
            {
                //some kind of indication that there would not be enough skill pts at the moment.
                SkillSoundSource.clip = SkillErrorSound;
                SkillSoundSource.PlayOneShot(SkillErrorSound);
            }
        }

        ////Delta prison input
        if (sk_DeltaPrison != 0 && skSelected == DELTAPRISON)
        {
            if (SkillPts >= DeltaPrisonCost && CurrSkill == NONE)
            {
                CurrSkill = DELTAPRISON;
                SkillPts -= DeltaPrisonCost;
                SkillSoundSource.clip = DeltaPrisonTriangleSound;
                SkillSoundSource.Play();
            }
            else
            {
                //some kind of indication that there would not be enough skill pts at the moment.
                SkillSoundSource.clip = SkillErrorSound;
                SkillSoundSource.PlayOneShot(SkillErrorSound);
            }
        }

        //Psi leap input
        if (sk_PsiLeap != 0 && skSelected == PSILEAP)
        {
            //get destination position and check for no obstacles
            float dirAngle_rad = MoveInnerInput.dir_lastFacedAngleDeg * 2 * Mathf.PI / 360;
            Vector3 movementAmt = new Vector3(Mathf.Cos(dirAngle_rad), Mathf.Sin(dirAngle_rad), 0) * 7;
            BoxCollider2D PsiLeapDestCollider = transform.Find("PsiLeapDestination").GetComponent<BoxCollider2D>();
            PsiLeapDestCollider.offset = movementAmt;
            Bounds PsiLeapDestBoundsZ0 = PsiLeapDestCollider.bounds;

            bool destIntersectsWithObstacle = false;

            foreach (GameObject obstacle in GameObject.FindGameObjectsWithTag("Obstacle"))
            {
                //use a position with a Z value of 0 for intersection

                Bounds ObstacleBoundsCollider = obstacle.GetComponentInChildren<BoxCollider2D>().bounds;

                Bounds ObstacleBoundsZ0 = ObstacleBoundsCollider;
                ObstacleBoundsZ0.center = new Vector3(ObstacleBoundsZ0.center.x, ObstacleBoundsZ0.center.y, 0);

                if (PsiLeapDestBoundsZ0.Intersects(ObstacleBoundsZ0)) //if within the region
                {
                    Debug.Log(obstacle.transform.parent.gameObject.name);
                    Debug.Log("Psi Leap dest intersects with obstacle");
                    destIntersectsWithObstacle = true;
                    break;
                }
            }


            if (SkillPts >= PsiLeapCost && CurrSkill == NONE && !destIntersectsWithObstacle)
            {
                CurrSkill = PSILEAP;
                PsiLeap_InitPosition = transform.position;
                SkillPts -= PsiLeapCost;

                SkillSoundSource.clip = PsiLeapDisappearSound;
                SkillSoundSource.Play();
            }
            else
            {
                if (CurrSkill != PSILEAP)
                {
                    //some kind of indication that there would not be enough skill pts at the moment.
                    SkillSoundSource.clip = SkillErrorSound;
                    SkillSoundSource.Play();
                }
            }
        }

        ////Temporal inhibition input
        if (sk_TemporalInhibition != 0 && skSelected == TEMPORALINHIBITION)
        {
            if (SkillPts >= TemporalInhibitionCost && CurrSkill == NONE)
            {
                CurrSkill = TEMPORALINHIBITION;
                SkillPts -= TemporalInhibitionCost;

                SkillSoundSource.clip = TemporalInhibitionStartSound;
                SkillSoundSource.PlayOneShot(TemporalInhibitionStartSound);
            }
            else
            {
                if (CurrSkill != TEMPORALINHIBITION)
                {
                    //some kind of indication that there would not be enough skill pts at the moment.
                    SkillSoundSource.clip = SkillErrorSound;
                    SkillSoundSource.Play();
                }
            }
        }
    }

    public void SkillActive()
    {
        if(CurrSkill == NONE)
        {
            //set alpha of white silhouette to 0, as this would be just when a skill would be in progress
            transform.Find("SpriteWhiteSilhouette").gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
            //set alpha to 0 for the Delta Prison ability
            transform.Find("DeltaPrisonTriangle").gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 0, 0);
            transform.Find("DeltaPrisonTriangleBlack").gameObject.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0);
        }
        //play out the active skill
        if (CurrSkill != NONE)
        {
            skillTimer += pDeltaTime; //update delta time INDEPENDENTLY OF timeScale for all skills, which would be fine as long as real time would be desirable for the players' skill durations

            //Psi Burst skill
            if (CurrSkill == PSIBURST)
            {
                //Damage all enemies within a certain radius of the player, apply a force pushing each of the enemies back away from the player
                GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

                //radius of Psi Burst is 2 + (level of skill - 1) * 0.4 units
                float AOEradius = 3 + (sk_PsiBurst - 1) * 0.4f;
                float AOEradiusSqr = AOEradius * AOEradius;


                foreach (GameObject enemy in enemies)
                {
                    //apply damage
                    Vector3 enemyPos = enemy.transform.position;

                    //get most root parent of GameObject that would be tagged as Enemy, as what would be to be moved
                    GameObject enemy_rootParent = enemy;
                    while (enemy_rootParent.transform.parent != null) {
                        enemy_rootParent = enemy.transform.parent.gameObject;
                    }

                    float sqrDistance = (enemyPos - transform.position).sqrMagnitude;
                    if (sqrDistance <= AOEradiusSqr)
                    {
                        enemy.GetComponentInChildren<EnemyHealth>().TakeDamage (20 + (sk_PsiBurst - 1) * 8); //damage is 20 + (level of skill - 1) * 8

                        //apply push
                        float pushMagnitude = 450; //as force units - noting how such would work with enemies that would be moving with eg. AI behaviours
                        Vector3 pushVectorNorm = Vector3.Normalize(enemy.transform.position - transform.position);
                        enemy_rootParent.GetComponentInChildren<Rigidbody2D>().AddForce(pushVectorNorm * pushMagnitude);
                        //could pause the animations of such enemies during such as well
                        enemy.GetComponentInChildren<EnemyMovement>().movementFreezeTimer += 0.45f;
                    }
                }

                //Have an animation with an independent timer of the PsiBurstRipples' circles increasing in radius at some rate until disappearing at the end of the animation's timer...well, can just create a prefab with such
                Instantiate(Resources.Load("PsiBurstRipples"), transform.position, transform.rotation);

                //play sound
                SkillSoundSource.clip = PsiBurstSound;
                SkillSoundSource.PlayOneShot(PsiBurstSound);

                CurrSkill = NONE;
                skillTimer = 0;
                PsiBurstImg.color = Color.white;

                //Maybe animation and\or something ... with such a Psi Burst ...
                //with maybe some sort of explosion
            }

            //if (CurrSkill == STIMPACK)
            //{
            //    playerShooting.shootingDelayScale = 0.5f * Mathf.Pow(0.95f, skillE - 1); //set to half * 0.95 ^ (level of skill - 1)
            //    StimPackImg.color = Color.red;
            //}
            if (CurrSkill == DELTAPRISON)
            {
                //startup and end time are scaled by level of skill
                float skStartup = 0.35f;
                float skActiveEnd = 0.6f;
                //Debug.Log("Delta Prison starting");
                if (skillTimer < skStartup)
                {
                    controllable = false; //uncontrollable during startup of skill
                    float silhouetteAlpha = (skillTimer / skStartup) > 1 ? 1 : (skillTimer / skStartup); //alpha being equivalent to the fraction of startup having passed
                    transform.Find("SpriteWhiteSilhouette").gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, silhouetteAlpha);
                    transform.Find("DeltaPrisonTriangle").gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 0, silhouetteAlpha);
                    transform.Find("DeltaPrisonTriangleBlack").gameObject.GetComponent<SpriteRenderer>().color = new Color(100f / 255f, 100f / 255f, 100f / 255f, silhouetteAlpha);
                    //Have maybe some triangle during such with transparency matching the polygon during such, as well ...
                }
                if (skillTimer >= skStartup && skillTimer < skActiveEnd)
                {
                    //cast skill
                    //make enemies within region effectively without movement nor attack for a period of time
                    //occurs only once - when controllable is false, where the occurrence changes controllable to true
                    if (!controllable)
                    {
                        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
                        {
                            //set Z value of the gameobject to be the same as the player's, check intersection, then revert Z value, or use a position with a Z value of 0 instead;
                            //doing the latter; for the latter, could create a copy of the Bounds of the respective GameObjects with Z-values of 0 (with setting such Z-values to 0), and then check intersection
                            Bounds PlayerBoundsZ0 = GetComponent<PolygonCollider2D>().bounds; //testing with BoxCollider2D
                            PlayerBoundsZ0.center = new Vector3(PlayerBoundsZ0.center.x, PlayerBoundsZ0.center.y, 0);
                            GameObject enemy_rootParent = enemy;
                            while (enemy_rootParent.transform.parent != null)
                            {
                                enemy_rootParent = enemy.transform.parent.gameObject;
                            }
                            Debug.Log("Enemy: "+enemy_rootParent.transform.name+", "+enemy.transform.childCount+", "+enemy.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite);

                            Bounds EnemyBoundsColliderTrigger = PlayerBoundsZ0; //initialize to something that should be changed / is to be changed; if it doesn't get changed, then the enemy would become frozen regardless of its position at least at some point within the period of freezing from the Delta Prison
                            foreach(BoxCollider2D enemyCollider in enemy.GetComponentsInChildren<BoxCollider2D>())
                            {
                                if(enemyCollider.isTrigger)
                                {
                                    EnemyBoundsColliderTrigger = enemyCollider.bounds;
                                }
                            }
                            Bounds EnemyBoundsZ0 = EnemyBoundsColliderTrigger; //set to bounds of the enemy's Collider2D that is a trigger (other OnTriggerEnter2D calls would be to cover the outer trigger already)
                            EnemyBoundsZ0.center = new Vector3(EnemyBoundsZ0.center.x, EnemyBoundsZ0.center.y, 0);
                            if (PlayerBoundsZ0.Intersects(EnemyBoundsZ0)) //if within the region
                            {
                                Debug.Log(enemy.transform.parent.gameObject.name);
                                float enemyFreezeMultiplier = 1;
                                if(enemy.GetComponentInChildren<EnemyHealth>().isBoss) //less effect on bosses
                                {
                                    enemyFreezeMultiplier = 1 / 3f;
                                }
                                enemy.GetComponent<EnemyMovement>().movementFreezeTimer = (4f + 2f * (sk_DeltaPrison - 1)) * enemyFreezeMultiplier; //set movement to freeze by adding to the freeze timer
                                                                                                                            //tint enemy during freeze for indicating such a freeze - done within the EnemyMovement component
                                enemy.GetComponent<EnemyMovement>().freezeColorTimer = (4f + 2f * (sk_DeltaPrison - 1)) * enemyFreezeMultiplier;
                                enemy.GetComponent<AILerp>().canMove = false;
                                Debug.Log("Delta Prison enemy found");
                            }
                        }
                        controllable = true;
                    }
                    //set alpha to decrease to 0, alpha equivalent to 1 - the fraction of the 'active' period having been passed
                    float sAOrig = 1 - (skillTimer - skStartup) / (skActiveEnd - skStartup);
                    float silhouetteAlpha = sAOrig < 0 ? 0 : sAOrig; //alpha being equivalent to the fraction of startup having passed
                    transform.Find("SpriteWhiteSilhouette").gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, silhouetteAlpha);
                    transform.Find("DeltaPrisonTriangle").gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 0, silhouetteAlpha);
                    transform.Find("DeltaPrisonTriangleBlack").gameObject.GetComponent<SpriteRenderer>().color = new Color(100f / 255f, 100f / 255f, 100f / 255f, silhouetteAlpha);
                    //Have maybe some triangle during such with transparency matching the polygon during such, as well ...
                    Debug.Log("Delta Prison active");
                }
                else if(skillTimer >= skActiveEnd)
                {
                    CurrSkill = NONE;
                    skillTimer = 0;
                    Debug.Log("Delta Prison done");
                }
            }

                if (CurrSkill == PSILEAP) //warp to location some distance in front of player
            {
                //noting of slight delay in the leap action, with disappearing and appearing gradually within a brief 0.3-0.5sec. or so, as what would be intended in the polish, with being intangible maybe during the period of 0.20-0.30sec. - basically teleporting to an inaccessible area as an easy way to implement this ...actually, such leading to camera being in such a place so disabling collider instead - then then warping to the closest position of the destination, where Unity's RigidBody and Physics2D implementation could handle how such would be positioned, and where alpha would be linearly related to the time up to 0.20 and between 0.30 to 0.50
                float skScale = 1 * Mathf.Pow(0.85f, sk_PsiLeap - 1); //scale factor for duration of the skill to be channeled

                if (skillTimer >= 0.20 * skScale && skillTimer <= 0.30 * skScale)
                {
                    GetComponent<BoxCollider2D>().enabled = false;
                }
                if(skillTimer >= 0.25 * skScale) //teleport at 0.25
                {
                    float dirAngle_rad = MoveInnerInput.dir_lastFacedAngleDeg * 2 * Mathf.PI / 360;
                    Vector3 movementAmt = new Vector3(Mathf.Cos(dirAngle_rad), Mathf.Sin(dirAngle_rad), 0) * 5;
                    transform.position = PsiLeap_InitPosition + movementAmt;

                    if(SkillSoundSource.clip != PsiLeapReappearSound) //flag for making the sound play only once during this skill
                    {
                        SkillSoundSource.clip = PsiLeapReappearSound;
                        SkillSoundSource.PlayOneShot(PsiLeapReappearSound);
                    }
                }
                if (skillTimer < 0.20 * skScale) // fade out - alpha decreases
                {
                    Color currColor = GetComponent<SpriteRenderer>().color;
                    GetComponent<SpriteRenderer>().color = new Color(currColor.r, currColor.g, currColor.b, 1 - (skillTimer / 0.20f));
                }
                else if (skillTimer > 0.30 * skScale && skillTimer < 0.50 * skScale) //fade in - alpha increases
                {
                    GetComponent<BoxCollider2D>().enabled = true;
                    Color currColor = GetComponent<SpriteRenderer>().color;
                    GetComponent<SpriteRenderer>().color = new Color(currColor.r, currColor.g, currColor.b, 1 - ((0.5f - skillTimer) / 0.20f));
                }
                else if (skillTimer >= 0.50 * skScale) //end skill
                {
                    GetComponent<BoxCollider2D>().enabled = true;
                    Color currColor = GetComponent<SpriteRenderer>().color;
                    GetComponent<SpriteRenderer>().color = new Color(currColor.r, currColor.g, currColor.b, 1);
                    PsiLeapImg.color = Color.white;
                    CurrSkill = NONE;
                    float dirAngle_rad = MoveInnerInput.dir_lastFacedAngleDeg * 2 * Mathf.PI / 360;
                    Vector3 movementAmt = new Vector3(Mathf.Cos(dirAngle_rad), Mathf.Sin(dirAngle_rad), 0) * 7;
                    transform.position = PsiLeap_InitPosition + movementAmt;
                    skillTimer = 0;
                    Debug.Log("Psi Leap done at " + skillTimer);
                }
                Debug.Log("Psi Leap at " + skillTimer);
            }

            if(CurrSkill == TEMPORALINHIBITION)
            {
                //Debug.Log("TI");
                Time.timeScale = 0.5f; //slow down the time around the player
                //maybe could also add an alpha transparent color on the world similar to that of the day\night cycle, which would fade in and out as the skill would be present
                Time.fixedDeltaTime = 1f / 50f * Time.timeScale; //adjust fixedDeltaTime so that the frame rate would still be the same - where the default physics frame rate would be 50 fps
                //would lead to FixedUpdate running at the same rate, though where the deltaTime regarding physics calculations would be less for each frame as what could be noted and so such actions that would be proportional to deltaTime behaving as so
                GetComponent<Animator>().speed = 1 / Time.timeScale; //adjust player animation
                
                //readjust all player movement and actions to increase their rates to be inverse of the timeScale elsewhere - though where as of 4/<rmh: 28/>25/18, 8:25PM, such would be where player movement would not be proporti<wl: o>nal to timeScale but just related to the FixedUpdate <<frame rate><YKWIM>>

                Color TIColor = TemporalInhibitionTransShade.GetComponent<SpriteRenderer>().color;
                TemporalInhibitionTransShade.GetComponent<SpriteRenderer>().color = new Color(TIColor.r, TIColor.g, TIColor.b, 0.4f);
                if (skillTimer >= (4.5 + (sk_TemporalInhibition - 1) * 1) - (TemporalInhibitionEndSound.length - 0.1f) && SkillSoundSource.clip != TemporalInhibitionEndSound/*  - used as a 'flag' for if the End sound would already have been played once*/) //at a point leading up to TI End, play the end sound for Temporal Inhibition
                {
                    SkillSoundSource.clip = TemporalInhibitionEndSound;
                    SkillSoundSource.PlayOneShot(TemporalInhibitionEndSound);
                }
                if (skillTimer >= 4.5 + (sk_TemporalInhibition-1) * 1)
                {
                    Debug.Log("TI End");
                    Time.timeScale = 1f; //slow down the time around the player - which physics would depend on
                    Time.fixedDeltaTime = 1f / 50f; //readjust fixedDeltaTime to revert to original frame rate
                    GetComponent<Animator>().speed = 1 / Time.timeScale;
                    PsiLeapImg.color = Color.white;
                    TemporalInhibitionTransShade.GetComponent<SpriteRenderer>().color = new Color(TIColor.r, TIColor.g, TIColor.b, 0f);
                    CurrSkill = NONE;
                    skillTimer = 0;
                }
            }
            
        }
        if(!controllable)
        {
            GetComponent<Animator>().enabled = false;
            transform.Find("SpriteWhiteSilhouette").gameObject.GetComponent<Animator>().enabled = false;
        }
        else
        {
            GetComponent<Animator>().enabled = true;
            transform.Find("SpriteWhiteSilhouette").gameObject.GetComponent<Animator>().enabled = true;
        }
    }

    // Update is called once per frame
    void Update () {
        //WeaponSelected();
        if(WaveManager.fadeScreenIsActive || PlayerHealth.playerIsDead)
        {
            return;
        }
        updateDeltaTime(); //update delta time for the player
        SkillActive();

        //regen SP
        if (SkillPts < MAX_SKILLPTS) { //hardcoded as 100
            SkillPts += skillPtsRegenRate * Time.deltaTime;
            if (SkillPts > MAX_SKILLPTS)
                SkillPts = MAX_SKILLPTS;

            //make skill points reflected on slider
        }
	}
}
