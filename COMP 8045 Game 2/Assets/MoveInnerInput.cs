using UnityEngine;
using System.Collections;

public class MoveInnerInput : MonoBehaviour {

    float GameObj_Radius;
    //public enum Dir { NEUTRAL, R, UR, U, UL, L, DL, D, DR }; //directions
    public static float dir_angleRad; //-100f would be neutral
    public static int dir_angleDeg;
    public static int dir_lastFacedAngleDeg;
    // Use this for initialization
    Vector3 Offset_FromOuterPartCenter = new Vector3(1f, -9f, 0f);
    public GameObject objWithFireShot;

    GameObject player;
    public static RuntimeAnimatorController[] playerAnimations; //taken from the player GameObject; animations from east at index 0, going CCW to SE at index 7 //made into a static var for access of player's i_selected value by Shadows, including at least in swapping with Shadows
    public static int i_selected; //made into a static var for access of player's i_selected value by Shadows, including at least in swapping with Shadows

    void Start () {
        GameObj_Radius = GetComponent<RectTransform>().rect.width *transform.localScale.x / 2;
        player = GameObject.FindGameObjectWithTag("Player");
        playerAnimations = player.GetComponent<TouchMove>().animations;

        i_selected = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (WaveManager.fadeScreenIsActive || PlayerHealth.playerIsDead) //disable while a fade screen is (at least 'to be') active / player is dead
        {
            return;
        }
        transform.position = OuterMovePart.OuterMovePart_Pos + Offset_FromOuterPartCenter; //default: back to original posn
        dir_angleRad = -100f; //default: back to neutral direction
        dir_angleDeg = -100; //<<angleDeg being the angle at which the closest point was found><YKWIM>>
        foreach (var touch in Input.touches) //Removed that was code here: for (var touch : Touch in Input.touches)
        {
            //check if input would be within the 'outer input move' region
            //checking if: 
            //(distance of touch from the center of the 'outer input move' region) + (the radius of the 'inner input move' region) would be less than (the radius of the 'outer input move' region)
            //Maybe better could be if: 
            //(distance of touch from the center of the 'outer input move' region) would be less than (the radius of the 'outer input move' region) + (the radius of the 'inner input move' region)
            float touchPos_z = Camera.main.transform.position.z;
            Vector3 touchPos_HUDCoords = /*Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, -touchPos_z))*/touch.position; //modified from being in world coords

            bool withinRegion = (touchPos_HUDCoords - OuterMovePart.OuterMovePart_Pos).magnitude < (OuterMovePart.GameObj_Radius + GameObj_Radius*8) / transform.localScale.x;
            
            //if so, then do such a movement of the respective inner input part
            //snap to the closest input area - among 8 points on the 'outer input move' region that would be separated equally and\or such by 45 degree angles

            if (withinRegion)
            {
                //Get each of the 8 points, foreach through them? As one way ... Getting positions through sin and cos of each angle, times the radius
                Vector3 min_dist_degAnglePoint_Posn = new Vector3(0, 0);
                float min_dist_degAnglePoint_dist = GameObj_Radius * 9 / transform.localScale.x; //some impossibly large distance as default
                //int i_selected; //made into a public member variable instead, for accessibility of the animation index for Shadows to make use of said index for facing direction
                for (int degAngle = 0; degAngle < 360; degAngle += 45)
                {
                    float degAngle_rad = degAngle * 2 * Mathf.PI / 360;

                    //position of one of the 8 points
                    Vector3 onePointPosition = (OuterMovePart.OuterMovePart_Pos +
                                          new Vector3(0f, (OuterMovePart.GameObj_Radius - GameObj_Radius) * Mathf.Sin(degAngle_rad), 0f) +
                                          new Vector3((OuterMovePart.GameObj_Radius - GameObj_Radius) * Mathf.Cos(degAngle_rad), 0f, 0f))
                                          ;
                    float dist_degAnglePoint = (touchPos_HUDCoords
                                          - onePointPosition).magnitude;
                    if (dist_degAnglePoint < min_dist_degAnglePoint_dist)
                    {
                        min_dist_degAnglePoint_dist = dist_degAnglePoint;
                        min_dist_degAnglePoint_Posn = onePointPosition; //does it copy? Well, it's passed by value according to someone, where such a person would have said that it's a structure
                        dir_angleRad = degAngle_rad;
                        dir_angleDeg = degAngle;
                        dir_lastFacedAngleDeg = degAngle;
                    }
                }
                i_selected = dir_angleDeg / 45; //corresponding index to the degAngle for animation setting

                //Check center of the move control as well
                Vector3 centerPointPosition = OuterMovePart.OuterMovePart_Pos;
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
                    if(!(objWithFireShot.GetComponent<FireShot>().isFiring)) //overridden by shooting animation direction
                        //set animator animation to the index corresponding to the <<angle at which the nearest point would have been found><YKWIM>>
                        player.GetComponentInChildren<Animator>().runtimeAnimatorController = playerAnimations[i_selected];
                }
                
                transform.position = min_dist_degAnglePoint_Posn + Offset_FromOuterPartCenter;
            }

            //and do any corresponding movement of eg. the player and/or scene and/or camera and/or such

        }
    }
}
