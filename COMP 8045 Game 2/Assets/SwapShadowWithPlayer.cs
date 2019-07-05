using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SwapShadowWithPlayer : MonoBehaviour, IPointerClickHandler
{
    GameObject player;
    Animator playerAnimator, thisAnimator;
    ShadowHealth shadowHealth;
    PlayerHealth playerHealth;
    ShadowPM PlayerTrainingPM;
    ShadowPM ThisShadowPM;
    GameObject OffScreenShadowPointer;

	// Use this for initialization
	void Start () {
        player = GameObject.FindGameObjectWithTag("Player");
        PlayerTrainingPM = player.transform.Find("ShadowCharacter_usedForLoggingTraining").GetComponentInChildren<ShadowPM>(); //PM for training, by player
        ThisShadowPM = GetComponent<ShadowPM>();

        playerAnimator = player.GetComponentInChildren<Animator>();
        thisAnimator = GetComponentInChildren<Animator>();
        shadowHealth = GetComponent<ShadowHealth>();
        playerHealth = player.GetComponent<PlayerHealth>();

        OffScreenShadowPointer = transform.Find("OffScreenShadowPointer").gameObject; //get OffScreenShadowPointer
    }
	
	//// Update is called once per frame
	void Update () {
        //become a selectable arrow when offscreen, or include that arrow as what could be selectable as part of this Shadow - with noting of what could be done with such of making it one of the Shadow's children without being targeted and\or such as part of eg. what would be considered as a ShadowCharacter and\or such

        //get arrow whose top-middle point would be positioned at the intersection of the line between the Shadow and the player and the edge of the screen
        //with said arrow being with its angle being such that the arrow would be directed along the line, towards the Shadow

        //rotate the arrow by the angle between the vector (0, 1) and (the vector from the player to the Shadow that would be with its Z value set to 0)
        //with such even if the top/forward end of the arrow would be the pivot
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if(PlayerHealth.playerIsDead || WaveManager.fadeScreenIsActive)
        {
            return;
        }
        //swap with player in:
        //-position
        Vector3 currPosition = transform.position;
        transform.position = player.transform.position;
        player.transform.position = currPosition;

        //-health
        int currHealth = shadowHealth.currHealth;
        shadowHealth.currHealth = playerHealth.currentHealth;
        playerHealth.currentHealth = currHealth;

        //-facing direction //though maybe not necessarily the same frame in the animation regarding the facing direction        
        thisAnimator.runtimeAnimatorController = ShadowPM.shadowAnimations[MoveInnerInput.i_selected];
        playerAnimator.runtimeAnimatorController = MoveInnerInput.playerAnimations[ThisShadowPM.i_selected];

        //reset PM cycles
        ThisShadowPM.SwappedPlacesThisUpdate = true;
        PlayerTrainingPM.SwappedPlacesThisUpdate = true;
    }
}
