using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class controls the climbing actions of the player.
public class ClimbController : MonoBehaviour
{
    ClimbPoint currentPoint;

    PlayerController playerController;
    EnviromentScanner scanner;

    // Awake is called when the script instance is being loaded.
    private void Awake()
    {
        // Fetch the attached EnviromentScanner component.
        scanner = GetComponent<EnviromentScanner>();

        // Fetch the attached PlayerController component.
        playerController = GetComponent<PlayerController>();
    }

    // Update is called once per frame.
    private void Update()
    {
        if (!playerController.isHang)
        {
            if (Input.GetButton("Jump") && !playerController.InAction)
            {
                // Check for ledges in front of the player.
                if (scanner.ClimbLedgeCheck(transform.forward, out RaycastHit ledgeHit))
                {
                    currentPoint = GetNearestClimbPoint(ledgeHit.transform, ledgeHit.point);
                    // Disable player controls.
                    playerController.SetControl(false);
                    // Start the jump-to-ledge animation.
                    StartCoroutine(JumpToLedge("IdleToHang", currentPoint.transform, 0.31f, 0.54f));
                }
            }
            if (Input.GetButton("Drop") && !playerController.InAction)
            {
                if(scanner.DropLedgeCheck(out RaycastHit ledgeHit))
                {
                    currentPoint = GetNearestClimbPoint(ledgeHit.transform, ledgeHit.point);

                    playerController.SetControl(false);
                    StartCoroutine(JumpToLedge("DropToHang", currentPoint.transform, 0.30f, 0.54f, handoffset: new Vector3(0.25f,0.2f,-0.2f)));

                }

            }
        }
        else
        {

            if (Input.GetButton("Drop") && !playerController.InAction)
            {
                StartCoroutine(JumpFromHang());
                return;

            }

            float horizontalInput = Mathf.Round(Input.GetAxis("Horizontal"));
            float verticalInput = Mathf.Round(Input.GetAxis("Vertical"));
            var inputDir = new Vector2(horizontalInput, verticalInput);

            if (playerController.InAction || inputDir == Vector2.zero) return;
            if(currentPoint.MountPoint && inputDir.y ==1)
            {
                StartCoroutine(MountFromHang());
                return;
            }

            if (currentPoint != null)
            {
               

                //Ledge to Ledge Jump

                var neighbour = currentPoint.GetNeighbour(inputDir);
                if (neighbour == null) return;

                if (neighbour.connectionType == ConnectionType.Jump && Input.GetButton("Jump"))
                {
                    currentPoint = neighbour.point;

                    if (neighbour.direction.y == 1)
                        StartCoroutine(JumpToLedge("HangHopUp", currentPoint.transform, 0.14f, 0.62f));

                    else if (neighbour.direction.y == -1)
                        StartCoroutine(JumpToLedge("HangHopDown", currentPoint.transform, 0.24f, 0.65f));
                    else if (neighbour.direction.x == 1)
                        StartCoroutine(JumpToLedge("HandHopRight", currentPoint.transform, 0.10f, 0.45f));
                    else if (neighbour.direction.x == -1)
                        StartCoroutine(JumpToLedge("HandHopLeft", currentPoint.transform, 0.10f, 0.45f));

                 
                }
                else if (neighbour.connectionType == ConnectionType.Move)
                {
                    currentPoint = neighbour.point;

                    if (neighbour.direction.x == 1)
                        StartCoroutine(JumpToLedge("ShimmyRight", currentPoint.transform, 0f, 0.38f, handoffset: new Vector3(0.25f,-0.10f, 0.1f)));
                    else if (neighbour.direction.x == -1)
                        StartCoroutine(JumpToLedge("ShimmyLeft", currentPoint.transform, 0.20f, 0.38f, AvatarTarget.LeftHand, handoffset: new Vector3(0.25f, - 
                            0.5f, 0.1f)));


                }

            }
            
        }
    }
  
    // Coroutine to handle the animation and positioning for the player to jump to a ledge.
    IEnumerator JumpToLedge(string anim, Transform ledge, float matchStartTime, float matchTargetTime, AvatarTarget hand = AvatarTarget.RightHand, Vector3? handoffset = null)
    {
        var matchParams = new MatchTargetParams()
        {
            // Define the position where the player's hand should be.
            pos = GetHandPos(ledge, hand, handoffset),

            // Target the right hand for matching.
            bodyPart = AvatarTarget.RightHand,

            // Define the start and end timings for matching.
            startTime = matchStartTime,
            targetTime = matchTargetTime,

            // Assign full weight for position match.
            posWeight = Vector3.one
        };

        // Calculate the required rotation to face the opposite direction of the ledge.
        var targetRot = Quaternion.LookRotation(-ledge.forward);

        // Perform the jump-to-ledge animation.
        yield return playerController.DoAction(anim, matchParams, targetRot, true);

        // Set the player's state to hanging after the animation.
        playerController.isHang = true;
    }

    // Calculate the position where the player's hand should grab the ledge.
    Vector3 GetHandPos(Transform ledge, AvatarTarget hand, Vector3? handoffSet)
    {

        var offVal = (handoffSet != null) ? handoffSet.Value : new Vector3(0.25f, -0.10f, 0.1f);
        var hDir = hand == AvatarTarget.RightHand ? ledge.right : -ledge.right;
        // Return the desired hand position relative to the ledge.
        return ledge.position + ledge.forward * 0.1f + Vector3.up * 0.10f - hDir * 0.25f;
    }
    IEnumerator JumpFromHang()
    {
        playerController.isHang = false;
        yield return playerController.DoAction("JumpFromHang");

        playerController.ResetTargetRotation();
        playerController.SetControl(true);
    }
    IEnumerator MountFromHang()
    {
        playerController.isHang = false;
        yield return playerController.DoAction("ClimbFromHang");
        playerController.EnableCharacterController(true);
        yield return new WaitForSeconds(0.5f);
        playerController.ResetTargetRotation();
        playerController.SetControl(true);
    }
    ClimbPoint GetNearestClimbPoint(Transform ledge, Vector3 hitPoint)
    {
        var points = ledge.GetComponentsInChildren<ClimbPoint>();


        ClimbPoint nearPoint = null;
        float nearestPointDistance = Mathf.Infinity;
        
        foreach (var point in points)
        {
            float distance = Vector3.Distance(point.transform.position, hitPoint);

            if (distance < nearestPointDistance)
            {
                nearPoint = point;
                nearestPointDistance = distance;
            }
        }
        return nearPoint;   

    }
}
