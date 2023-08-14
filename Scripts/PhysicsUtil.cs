using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsUtil
{
    public static bool ThreeRaycasts(Vector3 origim, Vector3 dir, float spacing, Transform transform,
        out List<RaycastHit> hits, float distance, LayerMask layer)
    {

       bool centerHitFound = Physics.Raycast(origim, Vector3.down, out RaycastHit centerHit, distance, layer);
       bool leftHitFound = Physics.Raycast(origim - transform.right * spacing, Vector3.down, out RaycastHit leftHit, distance, layer);
       bool rightHitFound = Physics.Raycast(origim + transform.right * spacing, Vector3.down, out RaycastHit rightHit, distance, layer);

        hits = new List<RaycastHit>() {centerHit, leftHit, rightHit }; 

       bool hitFound = centerHitFound || leftHitFound || rightHitFound;   

        return hitFound;
    }
 
}

    


