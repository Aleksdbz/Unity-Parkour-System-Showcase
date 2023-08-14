using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Parkour System/Custom Action/New vault action")]
public class Vault : ParKourAction
{

    public override bool CheckIfPossible(ObstacleHitData hitData, Transform player)
    {
        if (!base.CheckIfPossible(hitData, player))
        return false;

        var hitPoint = hitData.forwardHit.transform.InverseTransformPoint(hitData.forwardHit.point);

        if (hitPoint.z < 0 && hitPoint.x < 0 || hitPoint.z > 0 &&  hitPoint.x > 0)
        {
            mirror = true;
            matchBodyPart = AvatarTarget.RightFoot;
            
        }
        else
        {
            mirror = false;
            matchBodyPart = AvatarTarget.LeftHand;
        }
        return true;
    }

}
