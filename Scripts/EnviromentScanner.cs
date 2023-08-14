using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// This class provides utilities for checking the environment, 
// including obstacle and ledge detection.
public class EnviromentScanner : MonoBehaviour
{
    // Raycasting parameters for forward obstacle checks.
    [SerializeField] Vector3 forwardRayOffset = new Vector3(0, 0.25f, 0);
    [SerializeField] float forwardRayLenght = 0.8f;
    [SerializeField] float hieghtRayLenght = 5f;
    [SerializeField] float ledgeRayLenght = 10f;
    [SerializeField] LayerMask obstacleLayer;
    [SerializeField] float ledgeHeightTreshhold = 0.75f;

    // Parameters related to ledge detection during climbing.
    [Header("Ledge Detection")]
    [SerializeField] float climbLedgeRayLengh = 1.5f;
    [SerializeField] LayerMask climbLedgeLayer;

    // Checks if there's an obstacle in front and above the player.
    public ObstacleHitData ObstacleCheck()
    {
        var hitData = new ObstacleHitData();
        var forwardOrigin = transform.position + forwardRayOffset;

        hitData.forwardHitFound = Physics.Raycast(forwardOrigin, transform.forward, out hitData.forwardHit, forwardRayLenght, obstacleLayer);
        Debug.DrawRay(forwardOrigin, transform.forward * forwardRayLenght, hitData.forwardHitFound ? Color.red : Color.white);

        if (hitData.forwardHitFound)
        {
            var heightOrigin = hitData.forwardHit.point + Vector3.up * hieghtRayLenght;
            hitData.heightHitFound = Physics.Raycast(heightOrigin, Vector3.down, out hitData.heightHit, hieghtRayLenght, obstacleLayer);
            Debug.DrawRay(heightOrigin, Vector3.down * hieghtRayLenght, hitData.heightHitFound ? Color.red : Color.white);
        }
        return hitData;
    }

    // Checks for ledges that the player can climb.
    public bool ClimbLedgeCheck(Vector3 dir, out RaycastHit ledgeHit)
    {
        ledgeHit = new RaycastHit();

        // If no direction is provided, return false.
        if (dir == Vector3.zero)
            return false;

        var origin = transform.position + Vector3.up * 1.5f;
        var offSet = new Vector3(0, 0.18f, 0);

        // Checks in multiple vertical positions for a ledge.
        for (int i = 0; i < 10; i++)
        {
            Debug.DrawRay(origin + offSet * i, dir);
            if (Physics.Raycast(origin + offSet * i, dir, out RaycastHit hit, climbLedgeRayLengh, climbLedgeLayer))
            {
                ledgeHit = hit;
                return true;
            }
        }
        return false;
    }

    // Checks for obstacles at a specific height or angle that can be considered a ledge.
    public bool ObstacleLedgeCheck(Vector3 moveDir, out LedgeData ledgeData)
    {
        ledgeData = new LedgeData();

        // If no input is provided, don't check for a ledge.
        if (moveDir == Vector3.zero) return false;

        float originOffSet = 0.5f;
        var origin = transform.position + moveDir * originOffSet + Vector3.up;

        if (PhysicsUtil.ThreeRaycasts(origin, Vector3.down, 0.25f, transform, out List<RaycastHit> hits, ledgeRayLenght, obstacleLayer))
        {
            var validHits = hits.Where(h => transform.position.y - h.point.y > ledgeHeightTreshhold).ToList();

            if (validHits.Count > 0)
            {
                var surfaceRayOrigin = validHits[0].point;
                surfaceRayOrigin.y = transform.position.y - 0.1f;

                if (Physics.Raycast(surfaceRayOrigin, transform.position - surfaceRayOrigin, out RaycastHit surfaceHit, 2, obstacleLayer))
                {
                    float height = transform.position.y - validHits[0].point.y;

                    ledgeData.angle = Vector3.Angle(transform.forward, surfaceHit.normal);
                    ledgeData.height = height;

                    return true;
                }
            }
        }
        return false;
    }

    public bool DropLedgeCheck(out RaycastHit ledgeHit)
    {
        ledgeHit = new RaycastHit();

        var origin = transform.position + Vector3.down * 0.1f + transform.forward * 2f;

        if (Physics.Raycast(origin, -transform.forward, out RaycastHit hit, 3, climbLedgeLayer))
        {
            ledgeHit = hit;
            return true;
        }
        return false;
    }
}

// Data structure to store information about obstacles detected.
public struct ObstacleHitData
{
    public bool forwardHitFound;
    public bool heightHitFound;
    public RaycastHit forwardHit;
    public RaycastHit heightHit;
}

// Data structure to store information about ledges detected.
public struct LedgeData
{
    public float height;
    public float angle;
    public RaycastHit surfaceHit;
}
