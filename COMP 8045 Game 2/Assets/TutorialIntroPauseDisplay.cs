using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialIntroPauseDisplay : MonoBehaviour {

    public CanvasGroup[] tutorialExclusiveUICanvasGroups;
    public static int tutorialPage;
    public static int numTutorialPages;
    public CanvasGroup[] tutorialExclusiveUICanvasGroupPagesForNormalMode;
    public CanvasGroup[] tutorialExclusiveUICanvasGroupPagesForShadowMode;

    private CanvasGroup[] tutorialExclusiveUICanvasGroupPages;

    public static bool isActive
    {
        set
        {
            WaveManager.setIsActive(ref isActive_internal, value);
        }
        get
        {
            return isActive_internal;
        }
    }
    private static bool isActive_internal;
    private void Awake()
    {
        if(!WaveManager.isShadowMode)
        {
            tutorialExclusiveUICanvasGroupPages = tutorialExclusiveUICanvasGroupPagesForNormalMode;
        }
        else
        {
            tutorialExclusiveUICanvasGroupPages = tutorialExclusiveUICanvasGroupPagesForShadowMode;
        }
        numTutorialPages = tutorialExclusiveUICanvasGroupPages.Length;
    }

    // Use this for initialization
    void Start () {
	}
	
    /// <summary>
    /// Set alpha of all of the children of the canvas group if the first child's alpha - which is intended to be equivalent to the rest of the canvas renderer children's alphas in the canvas group (i.e. said alpha value being uniform across said canvas renderer children) - is not already that value.
    /// </summary>
    /// <param name="cGroup"></param>
    /// <param name="alpha"></param>
    public static void setAlphaOfChildren(CanvasGroup cGroup, float alpha)
    {
        CanvasRenderer firstCR = cGroup.gameObject.GetComponentInChildren<CanvasRenderer>(); //check first canvas renderer's alpha before setting all alphas
        if (firstCR.GetAlpha() != alpha)
        {
            //disable rendering for the items in the canvas group; an alternative way of doing this would be to disable the canvas, as well, though this code would be desired for individual canvas groups with separate rendering functionality/behaviour as well so implementing this this way for generalizability
            CanvasRenderer[] canvasRenderers = cGroup.gameObject.GetComponentsInChildren<CanvasRenderer>();
            foreach (CanvasRenderer cr in canvasRenderers)
            {
                cr.SetAlpha(alpha); //noting of if the canvasrenderer value would be set on setting the alpha of the canvas group, or if this would be a separate alpha value
            }
        }
    }

	// Update is called once per frame
	void Update () {
        if (isActive)
        {
            //fade in tutorial if not completed fading and is to be faded in
            foreach (CanvasGroup cGroup in tutorialExclusiveUICanvasGroups)
            {
                //increase alpha
                if (cGroup.alpha < 1)
                {
                    cGroup.alpha += Time.deltaTime / Shop.shopFadeTransitionDuration; //add the alpha proportional to the fraction of the set transition duration
                }
                if (cGroup.alpha > 1)
                {
                    cGroup.alpha = 1; //if gone to greater than 1, then set to 1
                }
                setAlphaOfChildren(cGroup, 1);
            }

            //fading of separate canvas group pages
            for (int i = 0; i < tutorialExclusiveUICanvasGroupPages.Length; i++)
            {
                int iPage = i + 1;
                CanvasGroup cGroup = tutorialExclusiveUICanvasGroupPages[i];

                //if the current tutorial page is the page corresponding to the canvas group at the current index in this loop, then - if possible - increase said canvas group's alpha to 1; otherwise decrease it to 0
                if (tutorialPage == iPage)
                {
                    //increase alpha
                    if (cGroup.alpha < 1)
                    {
                        cGroup.alpha += Time.deltaTime / Shop.shopFadeTransitionDuration; //add the alpha proportional to the fraction of the set transition duration
                    }
                    if (cGroup.alpha > 1)
                    {
                        cGroup.alpha = 1; //if gone to greater than 1, then set to 1
                    }

                    setAlphaOfChildren(cGroup, 1);
                }
                else
                {
                    if (cGroup.alpha > 0)
                    {
                        cGroup.alpha -= Time.deltaTime / Shop.shopFadeTransitionDuration; //subtract the alpha proportional to the fraction of the set transition duration
                    }
                    if (cGroup.alpha < 0)
                    {
                        cGroup.alpha = 0; //if gone to less than 0, then set to 0
                    }

                    if (cGroup.alpha <= 0)
                    {
                        setAlphaOfChildren(cGroup, 0);
                    }
                    else
                    {
                        setAlphaOfChildren(cGroup, 1);
                    }
                }
            }

            //make interactable
            GetComponent<CanvasGroup>().blocksRaycasts = true;
        }
        else
        {
            foreach (CanvasGroup cGroup in tutorialExclusiveUICanvasGroups)
            {
                if (cGroup.alpha > 0)
                {
                    //testing if alpha value of a separate canvas renderer component of an obj would be separate from eg. a canvas group's alpha
                    CanvasRenderer aCanvasRenderer = cGroup.gameObject.GetComponentInChildren<CanvasRenderer>();
                    cGroup.alpha -= Time.deltaTime / Shop.shopFadeTransitionDuration; //subtract the alpha proportional to the fraction of the set transition duration
                }
                if (cGroup.alpha < 0)
                {
                    cGroup.alpha = 0; //if gone to less than 0, then set to 0
                }
                if(cGroup.alpha <= 0)
                {
                    setAlphaOfChildren(cGroup, 0);
                }
                else
                {
                    setAlphaOfChildren(cGroup, 1);
                }
            }

            //make uninteractable
            GetComponent<CanvasGroup>().blocksRaycasts = false;

        }
    }
}
