using UnityEngine;
using System.Collections;

public class ShotInnerInput : MonoBehaviour
{

    float GameObj_Radius;
    public static float dir_angleRad; //-100f would be neutral
    public static int dir_angleDeg;
    public static int dir_lastFacedAngleDeg;
    // Use this for initialization
    Vector3 Offset_FromOuterPartCenter = new Vector3(1f, -9f, 0f);

    GameObject player;
    private RuntimeAnimatorController[] playerAnimations; //taken from the player GameObject; animations from east at index 0, going CCW to SE at index 7

    void Start()
    {
        GameObj_Radius = GetComponent<RectTransform>().rect.width *transform.localScale.x / 2;
        player = GameObject.FindGameObjectWithTag("Player");
        playerAnimations = player.GetComponent<TouchMove>().animations;
    }

    // Update is called once per frame
    void Update()
    {
        if (WaveManager.fadeScreenIsActive || PlayerHealth.playerIsDead) //disable while a fade screen is active / player is dead
        {
            return;
        }
        transform.position = OuterShotPart.OuterShotPart_Pos + Offset_FromOuterPartCenter; //default: back to original posn
        dir_angleRad = -100f; //default: back to neutral direction
        dir_angleDeg = -100; //<<angleDeg being the angle at which the closest point was found><YKWIM>>

        FireShot fireShot = GetComponent<FireShot>();
        bool prevFireShotisFiring = fireShot.isFiring;
        //<<Given><YKWIM>> that this class is only used by a player, will not check forPlayer <<and/or such of eg. fireShot><YKWIM>>
        if (!(Input.GetKey("i") || Input.GetKey("j") || Input.GetKey("k") || Input.GetKey("l"))) //if keys not pressed, then allow initialization to false
        {
            fireShot.isFiring = false;
        }
        foreach (var touch in Input.touches)
        {
            //check if input would be within the 'outer input move' region
            float touchPos_z = Camera.main.transform.position.z;
            Vector3 touchPos_HUDCoords = /*Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, -touchPos_z))*/touch.position; //modified from being in world coords

            bool withinRegion = (touchPos_HUDCoords - OuterShotPart.OuterShotPart_Pos).magnitude < (OuterShotPart.GameObj_Radius + GameObj_Radius * 7) / transform.localScale.x;
            
            //if so, then do such a movement of the respective inner input part
            //snap to the closest input area - among 8 points on the 'outer input move' region that would be separated equally by 45 degree angles

            if (withinRegion)
            {
                //Get each of the 8 points, foreach through them? As one way ... Getting positions through sin and cos of each angle, times the radius
                Vector3 min_dist_degAnglePoint_Posn = new Vector3(0, 0);
                float min_dist_degAnglePoint_dist = GameObj_Radius * 8 / transform.localScale.x; //some impossibly large distance as default
                int i_selected;
                for (int degAngle = 0; degAngle < 360; degAngle += 45)
                {
                    float degAngle_rad = degAngle * 2 * Mathf.PI / 360;

                    //position of one of the 8 points
                    Vector3 onePointPosition = (OuterShotPart.OuterShotPart_Pos +
                                          new Vector3(0f, (OuterShotPart.GameObj_Radius - GameObj_Radius) * Mathf.Sin(degAngle_rad), 0f) +
                                          new Vector3((OuterShotPart.GameObj_Radius - GameObj_Radius) * Mathf.Cos(degAngle_rad), 0f, 0f))
                                          ;
                    float dist_degAnglePoint = (touchPos_HUDCoords
                                          - onePointPosition).magnitude;
                    if (dist_degAnglePoint < min_dist_degAnglePoint_dist)
                    {
                        min_dist_degAnglePoint_dist = dist_degAnglePoint;
                        min_dist_degAnglePoint_Posn = onePointPosition;
                        dir_angleRad = degAngle_rad;
                        dir_angleDeg = degAngle;
                        dir_lastFacedAngleDeg = degAngle;
                    }
                }
                i_selected = dir_angleDeg / 45; //corresponding index to the degAngle for animation setting

                //Check center of the move control as well
                Vector3 centerPointPosition = OuterShotPart.OuterShotPart_Pos;
                float dist_center = (touchPos_HUDCoords
                                          - centerPointPosition).magnitude;
                if (dist_center < min_dist_degAnglePoint_dist)
                {
                    min_dist_degAnglePoint_dist = dist_center;
                    min_dist_degAnglePoint_Posn = centerPointPosition; //does it copy? Well, it's passed by value according to someone, where such a person would have said that it's a structure
                    dir_angleRad = -100f;
                    dir_angleDeg = -100;
                }
                else //if the center point would not be the closest point
                {
                        //set animator animation to the index corresponding to the <<angle at which the nearest point would have been found><YKWIM>>
                        Animator animator = player.GetComponentInChildren<Animator>();
                        if (animator.runtimeAnimatorController != playerAnimations[i_selected]) //if new direction set
                        {
                            FireShot.laserFollowupShot = false; //not a followup shot for laser with having this changed direction
                        Debug.Log("new direction set");
                            animator.runtimeAnimatorController = playerAnimations[i_selected];
                        }
                }
                //
                fireShot.isFiring = true;
                transform.position = min_dist_degAnglePoint_Posn + Offset_FromOuterPartCenter;
            }

            //and do any corresponding movement of eg. the player and\or scene<< and\or camera><YKWIM>> and\or such

        }
        if(prevFireShotisFiring && !fireShot.isFiring) //on exit
        {
            FireShot.laserFollowupShot = false;
            Debug.Log("end laser followup shot; shotCooldown of player: "+FireShot.shotCooldown);
        }
        if(!prevFireShotisFiring && fireShot.isFiring) //on enter
        {
            fireShot.FireShotEnter();
        }
    }
}

