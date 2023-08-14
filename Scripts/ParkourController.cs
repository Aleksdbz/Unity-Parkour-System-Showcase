using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ParkourController : MonoBehaviour 
{
    [SerializeField] List<ParKourAction> parkourActions; // List of possible parkour actions.
    [SerializeField] ParKourAction jumpDownAction;
    EnviromentScanner enviromentScanner;
    Animator animator;
    PlayerController playerController;
   

    private void Awake()
    {
        enviromentScanner = GetComponent<EnviromentScanner>();
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();

    }
    private void Update()
    {
        // Check for obstacles.
        var hitData = enviromentScanner.ObstacleCheck();
        // If the player hits the jump button and is not currently in a parkour action.
        if (Input.GetButton("Jump") && !playerController.InAction && !playerController.isHang)
        {
         
            // If there is an obstacle.
            if (hitData.forwardHitFound)
            {
                // Go through each parkour action.
                foreach (var action in parkourActions)
                {
                    // If the action is possible, perform it and stop checking other actions.
                    if (action.CheckIfPossible(hitData, transform))
                    {
                        StartCoroutine(DoparkourAction(action));
                        break;
                    }
                }  
            }

            if(playerController.IsOnLedge && !playerController.InAction && !hitData.forwardHitFound)
            {

                if(playerController.LedgeData.angle <= 50)
                {
                    playerController.IsOnLedge = false;
                    StartCoroutine(DoparkourAction(jumpDownAction));
                }
            }
        }
   
          // Perform a parkour action.
        IEnumerator DoparkourAction(ParKourAction action)
        {
            playerController.SetControl(false); // Disable player control during the parkour action.

            MatchTargetParams matchParam = null;
            if (action.EnableTargetMatching)
            {
                matchParam = new MatchTargetParams
                {
                    pos = action.matchPosition,
                    bodyPart = action.MatchBodyPart,
                    posWeight = action.MatchPosWeight,
                    startTime = action.MatchStartTime,
                    targetTime = action.MatchTargetTime
                };
            }


            yield return  playerController.DoAction(action.AnimName, matchParam, action.TargetRotation, action.RotateTowardsObstacle, action.postActionDelay, action.mirror);

            playerController.SetControl(true); // Re - enable player control after the action is done.
           
        }

#pragma warning disable CS8321 // Local function is declared but never used
        void MatchTarget(ParKourAction action)
        {
            // Don't call MatchTarget if a match operation is ongoing.
            if (animator.isMatchingTarget) return;

            if (!animator.IsInTransition(0))
            {
                animator.MatchTarget(action.matchPosition, Quaternion.identity, action.MatchBodyPart,
                                   new MatchTargetWeightMask(action.MatchPosWeight,0), action.MatchStartTime, action.MatchTargetTime);
            }       
        }
#pragma warning restore CS8321 // Local function is declared but never used
    }


    
}
