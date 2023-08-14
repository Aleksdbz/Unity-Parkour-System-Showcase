
using UnityEngine;

[CreateAssetMenu(menuName ="Parkour System/New parkour action")]
public class ParKourAction : ScriptableObject
{
    [SerializeField] string animName;
    [SerializeField] string obstacleTag;
    
    //PERFORM PARKOUT ACTION IF HEIGHT OF OBSTACLE IS BETWEEN MIN HEIGHT AND MAX HEIGHT
    [SerializeField] float minHeight;
    [SerializeField] float maxHeight;
    [SerializeField] bool rotateTowardsObstacle;
    [SerializeField] float PostActionDelay;
   


    [Header("Target Matching")]
    [SerializeField] bool enableTargetMatching = true;
    [SerializeField] protected AvatarTarget matchBodyPart;
    [SerializeField] float matchStartTime;
    [SerializeField] float matchTargetTime;
    [SerializeField] Vector3 matchPosWeight = new Vector3(0,1,0);   
    public Quaternion TargetRotation { get; set; } // The target rotation for the player if rotateTowardsObstacle is true.
    public Vector3 matchPosition { get; set; }
    public bool mirror;


    // Check if a parkour action is possible based on the height of an obstacle and player position.
    public virtual bool CheckIfPossible(ObstacleHitData hitData, Transform player)
    {
        if (!string.IsNullOrEmpty(obstacleTag) && !hitData.forwardHit.transform.CompareTag(obstacleTag))
            return false;
        // Calculate the height of the obstacle.
        float height = hitData.heightHit.point.y - player.position.y;
        // If the height is not within the minHeight and maxHeight, return false.
        if (height < minHeight || height > maxHeight)
            return false;
        // If we should rotate towards the obstacle, calculate the target rotation.
        if (rotateTowardsObstacle)
            TargetRotation = Quaternion.LookRotation(-hitData.forwardHit.normal);

        if (enableTargetMatching)
            matchPosition = hitData.heightHit.point;
        // If the code gets this far, the action is possible, so return true.
        return true;
        
    }
    // Properties to access private serialized fields.
    public string AnimName => animName;
    public bool RotateTowardsObstacle => rotateTowardsObstacle;
    public float postActionDelay => PostActionDelay;
    public bool EnableTargetMatching => enableTargetMatching;
    public AvatarTarget MatchBodyPart => matchBodyPart;
    public float MatchStartTime => matchStartTime;
    public float MatchTargetTime => matchTargetTime;
    public Vector3 MatchPosWeight => matchPosWeight;
}

