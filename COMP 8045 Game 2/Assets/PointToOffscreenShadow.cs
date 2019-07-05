using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PointToOffscreenShadow : MonoBehaviour {

    //public bool isOnScreen;
    public SpriteRenderer SpriteToCheckIfOffscreen;
    public ShadowHealth ThisShadowHealth;
    public GameObject player;

    static float viewportLeftXOrigin, viewportRightXOrigin, viewportTopYOrigin, viewportBottomYOrigin;

    static bool viewportAtOriginSet = false; //for what would be done once in running the project? With noting of such on 2/7/19

    private void Awake()
    {
        if (!viewportAtOriginSet)
        {
            viewportLeftXOrigin = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane)).x;
            viewportRightXOrigin = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, Camera.main.nearClipPlane)).x;
            viewportTopYOrigin = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, Camera.main.nearClipPlane)).y;
            viewportBottomYOrigin = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane)).y;
            viewportAtOriginSet = true;
        }
    }

    // Use this for initialization
    void Start () {
        player = GameObject.FindGameObjectWithTag("Player");
        SpriteToCheckIfOffscreen = transform.parent.gameObject.GetComponent<SpriteRenderer>();
        if (SpriteToCheckIfOffscreen.isVisible)
        {
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<PolygonCollider2D>().enabled = false;
        }
        else
        {
            GetComponent<SpriteRenderer>().enabled = true;
            GetComponent<PolygonCollider2D>().enabled = true;
        }
        ThisShadowHealth = transform.parent.gameObject.GetComponent<ShadowHealth>();
    }

    /// <summary>
    /// Return whether the vector intersects with the vertical line segment, and also set yIntersect to the intersecting y value found with the line x = lineX.
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="lineX"></param>
    /// <param name="topofLineSeg"></param>
    /// <param name="bottomofLineSeg"></param>
    /// <param name="yIntersect"></param>
    /// <returns></returns>
    bool VecIntersectsVerticalLineSeg(Vector3 vec, float lineX, float topofLineSeg, float bottomofLineSeg, ref float yIntersect)
    {
        if(lineX >= 0 && vec.x >= lineX || lineX <= 0 && vec.x <= lineX) //if the vector extends to the other side of the line; note that vec.x cannot be 0 for intersection with the viewport
        {
            //check if the y value is within the line segment range
            yIntersect = vec.y / vec.x * lineX; //find the point of the vector, as a line y = mx + b where b = 0, at where x = lineX
            return yIntersect <= topofLineSeg && yIntersect >= bottomofLineSeg;
        }
        return false;
    }

    /// <summary>
    /// Return whether the vector intersects with the horizontal line segment, and also set xIntersect to the intersecting x value found with the line y = lineY.
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="lineY"></param>
    /// <param name="leftofLineSeg"></param>
    /// <param name="rightofLineSeg"></param>
    /// <param name="xIntersect"></param>
    /// <returns></returns>
    bool VecIntersectsHorizontalLineSeg(Vector3 vec, float lineY, float leftofLineSeg, float rightofLineSeg, ref float xIntersect)
    {
        if (lineY >= 0 && vec.y >= lineY || lineY <= 0 && vec.y <= lineY) //if the vector extends to the other side of the line; note that vec.y cannot be 0 for intersection with the viewport
        {
            //check if the y value is within the line segment range
            xIntersect = lineY / vec.y * vec.x; //find the point of the vector, as a line y = mx + b where b = 0, at where y = lineY
            return xIntersect >= leftofLineSeg && xIntersect <= rightofLineSeg;
        }
        return false;
    }

    // Update is called once per frame
    void Update () {

        //become a selectable arrow when offscreen, or include that arrow as what could be selectable as part of this Shadow - with noting of what could be done with such of making it one of the Shadow's children without being targeted and\or such as part of eg. what would be considered as a ShadowCharacter and\or such

        //disable (or enable, depending on the visibility of the sprite to check if offscreen,) raycasting and visibility of this pointer
        if (SpriteToCheckIfOffscreen.isVisible)
        {
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<PolygonCollider2D>().enabled = false;
            //Debug.Log("Shadow sprite is visible");
        }
        else
        {
            GetComponent<SpriteRenderer>().enabled = true;
            GetComponent<PolygonCollider2D>().enabled = true;

            //get arrow whose top-middle point would be positioned at the intersection of the line between the Shadow and the player and the edge of the screen
            //with said arrow being with its angle being such that the arrow would be directed along the line, towards the Shadow

            Vector3 playerToShadow = transform.parent.position - player.transform.position;
                //check intersection of playerToShadow with y = top of viewport's y, y = bottom of viewport's y, x = left of viewport's x, x = right of viewport's x
                //and determine point of intersection with <<what would have been intersected with><YKWIM>> in said lines

                //checking of if m is infinity in y = mx + b
                //where m = (playerToShadow.y / playerToShadow.x) and b = 0
                //and so, in solving for y = mx + b and y = top of viewport's y, 
                //x = top of viewport's y * (playerToShadow.x / playerToShadow.y)
                //and if playerToShadow.x is 0, noting of the x and being 0
                //and if y would be above the viewport and x would be within the range of the left and right of the viewport, then the line would intersect

            //checking intersection with all sides of the viewport
            float xIntersect = 0;
            float yIntersect = 0;

            if (VecIntersectsVerticalLineSeg(playerToShadow, viewportLeftXOrigin, viewportTopYOrigin, viewportBottomYOrigin, ref yIntersect))
            {
                xIntersect = viewportLeftXOrigin;
            }
            else if(VecIntersectsVerticalLineSeg(playerToShadow, viewportRightXOrigin, viewportTopYOrigin, viewportBottomYOrigin, ref yIntersect))
            {
                xIntersect = viewportRightXOrigin;
            }
            else if (VecIntersectsHorizontalLineSeg(playerToShadow, viewportTopYOrigin, viewportLeftXOrigin, viewportRightXOrigin, ref xIntersect))
            {
                yIntersect = viewportTopYOrigin;
            }
            else if (VecIntersectsHorizontalLineSeg(playerToShadow, viewportBottomYOrigin, viewportLeftXOrigin, viewportRightXOrigin, ref xIntersect))
            {
                yIntersect = viewportBottomYOrigin;
            }
            else
            {
                //no intersection found ...shouldn't happen unless the sprite would be within the bounds but disabled
                Debug.Log("no intersection found. This shouldn't occur since this should have been handled earlier! viewport leftX: "+viewportLeftXOrigin+", rightX: "+viewportRightXOrigin+", topY: "+viewportTopYOrigin+", bottomY: "+viewportBottomYOrigin);
            }

            transform.position = (Vector3)((Vector2)Camera.main.transform.position) + new Vector3(xIntersect, yIntersect, transform.position.z);

            //rotate the arrow by the angle between the vector (0, 1) and (the vector from the player to the Shadow that would be with its Z value set to 0)
            //with such even if the top/forward end of the arrow would be the pivot
                float angle_relativeToInitialAngle = (Vector2.SignedAngle(Vector2.up, (Vector2)playerToShadow) + 360) % 360; //get CCW angle from Vector2.up to playerToShadow
                transform.eulerAngles = new Vector3(0f, 0f, angle_relativeToInitialAngle); //adjust by the CCW angle

            //make color of arrow depend on the Shadow's health, from red at full health to black when at 0 health with such of a relationship that would be a linear relationship - where YKWIM by this
            //This would be done via setting the color of the sprite renderer, from a white color to black color of the spriterenderer, where the actual sprite would be red - where YKWIM by this
            float currHealthFraction = (float)(ThisShadowHealth.currHealth) / ThisShadowHealth.maxHealth;
            GetComponent<SpriteRenderer>().color = new Color(currHealthFraction, currHealthFraction, currHealthFraction);
        }
    }
}
