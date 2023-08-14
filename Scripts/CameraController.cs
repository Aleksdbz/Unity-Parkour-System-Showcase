using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    [SerializeField] Transform followTarget;
    [SerializeField] float distance = 5f;
    [SerializeField] float minVerticalAngle = -45f;
    [SerializeField] float maxVerticalAngle = 45f;
    [SerializeField] Vector2 framingOffSet;
    [SerializeField] float rotationSpeed = 2f;
    [SerializeField] bool invertX;
    [SerializeField] bool invertY;
    float rotationY;
    float rotationX;
    float invertXval;
    float invertYval;

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;   
    }
    private void Update()
    {

         invertXval =  (invertX)? -1 : 1;
         invertYval =  (invertX)? -1 : 1;

         rotationX += Input.GetAxis("Camera Y")* invertYval * rotationSpeed;
         rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);
         rotationY += Input.GetAxis("Camera X") * invertXval * rotationSpeed;


         var targetRotation = Quaternion.Euler(rotationX, rotationY, 0);
         var focusPosition = followTarget.position + new Vector3(framingOffSet.x, framingOffSet.y);

        transform.position = focusPosition - targetRotation  *  new Vector3(0,0, distance);
        transform.rotation = targetRotation;
    }

    public Quaternion PlaneRotation => Quaternion.Euler(0, rotationY, 0);

}
