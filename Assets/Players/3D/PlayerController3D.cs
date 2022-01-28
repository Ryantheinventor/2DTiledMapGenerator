using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.InputSystem;

public class PlayerController3D : MonoBehaviour
{
    public float topSpeed = 3f;
    public float acceleration = 3f;
    public float inAirAcceleration = 3f;
    public float jumpPower = 3f;
    public float skinWidth = 0.001f;
    public float maxGroundAngle = 45f;
    public float friction = 1f;
    public Vector3 velocity = new Vector3();
    private Rigidbody rb;
    private BoxCollider bc;
    public bool grounded = false;
    private Vector3 moveVector;
    public Vector3 curGroundNormal = Vector3.up;
    public float sensitivity = 1f;
    public Transform camera;
    public static bool playerActive = true;
    public LayerMask layerMask;

    public float cameraFHeight = 100;

    //private void OnEnable()
    //{
    //    StaticPlayerInput.PInput.currentActionMap["Move"].started += OnMove;
    //    StaticPlayerInput.PInput.currentActionMap["Move"].performed += OnMove;
    //    StaticPlayerInput.PInput.currentActionMap["Move"].canceled += OnMove;
    //    StaticPlayerInput.PInput.currentActionMap["Jump"].started += OnJump;
    //}

    //private void OnDisable()
    //{
    //    StaticPlayerInput.PInput.currentActionMap["Move"].started -= OnMove;
    //    StaticPlayerInput.PInput.currentActionMap["Move"].performed -= OnMove;
    //    StaticPlayerInput.PInput.currentActionMap["Move"].canceled -= OnMove;
    //    StaticPlayerInput.PInput.currentActionMap["Jump"].started -= OnJump;
    //}

    private void Start()
    {

        rb = GetComponent<Rigidbody>();
        bc = GetComponent<BoxCollider>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    //private void OnMove(InputAction.CallbackContext obj)
    //{
    //    if (playerActive)
    //    {
    //        Vector3 input = obj.ReadValue<Vector2>();
    //        moveVector = new Vector3(input.x, 0, input.y);
    //    }
    //}

    //private void OnJump(InputAction.CallbackContext obj)
    //{
    //    if (grounded && playerActive)
    //    {
    //        velocity += new Vector3(0, jumpPower, 0);
    //    }

    //}

    private void Update()
    {
        


        //call old events
        moveVector = new Vector3();
        if (Input.GetKey(KeyCode.A))
        {
            moveVector += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveVector += new Vector3(1, 0, 0);
        }
        if (Input.GetKey(KeyCode.W))
        {
            moveVector += new Vector3(0, 0, 1);
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveVector += new Vector3(0, 0, -1);
        }        

        if (grounded && playerActive && Input.GetKeyDown(KeyCode.Space))
        {
            velocity += new Vector3(0, jumpPower, 0);
        }

        if (playerActive)
        {
            //Vector2 mDelta = Mouse.current.delta.ReadValue() * 0.05f;
            Vector2 mDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * 0.5f;
            rb.rotation = rb.rotation * Quaternion.Euler(new Vector3(0, mDelta.x * Time.deltaTime * sensitivity * 140, 0));
            camera.transform.rotation = camera.transform.rotation * Quaternion.Euler(new Vector3(-mDelta.y * Time.deltaTime * sensitivity * 140, 0, 0));
            if(Input.GetKey(KeyCode.F))
            {
                camera.transform.localPosition = new Vector3(0,Mathf.Lerp(camera.transform.localPosition.y,cameraFHeight,5*Time.deltaTime),0);
            }
            else
            {
                camera.transform.localPosition = new Vector3(0,Mathf.Lerp(camera.transform.localPosition.y,0.5f,5*Time.deltaTime),0);
            }
        }
    }

    private void FixedUpdate()
    {
        if (playerActive)
        {
            Vector3 curMoveVec = Vector3.ProjectOnPlane(Quaternion.FromToRotation(Vector3.forward, transform.forward) * moveVector, curGroundNormal); //cur movement vector rotated to match player facing direction
            Debug.DrawRay(transform.position, curMoveVec, Color.blue);
            //player input
            if (grounded)
            {
                velocity += new Vector3(curMoveVec.x * acceleration * Time.fixedDeltaTime, curMoveVec.y * acceleration * Time.fixedDeltaTime, curMoveVec.z * acceleration * Time.fixedDeltaTime);
            }
            else
            {
                velocity += new Vector3(curMoveVec.x * inAirAcceleration * Time.fixedDeltaTime, curMoveVec.y * inAirAcceleration * Time.fixedDeltaTime, curMoveVec.z * inAirAcceleration * Time.fixedDeltaTime);
            }

            Vector2 cv = new Vector2(velocity.x, velocity.z);
            if (cv.magnitude > topSpeed)
            {
                float why = velocity.y;
                velocity = velocity.normalized * topSpeed;
                velocity = new Vector3(velocity.x,why,velocity.z);
            }
            bool accelerating = false;
            if (moveVector.magnitude > 0)
            {
                accelerating = true;
            }

            //gravity
            velocity += Physics.gravity * Time.fixedDeltaTime;

            Vector3 newPos = rb.position + (velocity * Time.fixedDeltaTime);

            //collision check
            Vector3 boxSize = bc.size;
            Vector3 objectScale = transform.lossyScale;
            Vector3 finalBoxSize = new Vector3(boxSize.x * objectScale.x + skinWidth, boxSize.y * objectScale.y + skinWidth, boxSize.z * objectScale.z + skinWidth);
            Collider[] overlaps = Physics.OverlapBox(newPos, finalBoxSize / 2 + new Vector3(0, 0.2f, 0), rb.rotation, layerMask, QueryTriggerInteraction.Ignore);

            //MTV/velocity clipping
            grounded = false;
            foreach (Collider other in overlaps)
            {
                if (other != bc)
                {
                    if (Physics.ComputePenetration(bc, newPos, rb.rotation,
                        other, other.transform.position, other.transform.rotation,
                        out Vector3 direction, out float distance))
                    {
                        if (Vector3.Angle(Vector3.up, direction) < maxGroundAngle)
                        {
                            grounded = true;
                            curGroundNormal = direction;
                            newPos += direction * distance + new Vector3(-direction.x, 0, -direction.z) * distance;
                            Debug.DrawRay(transform.position, direction * distance, Color.green);
                            Debug.DrawRay(transform.position + direction * distance, new Vector3(-direction.x, 0, -direction.z) * distance, Color.green);
                            velocity = Vector3.ProjectOnPlane(velocity, Vector3.up);
                        }
                        else
                        {
                            newPos += direction * distance;
                            velocity = Vector3.ProjectOnPlane(velocity, direction);
                        }

                    }

                }
            }

            if (grounded && !accelerating)
            {
                Vector3 vDirection = velocity.normalized;
                Vector3 deceleration = vDirection * friction;
                if (deceleration.magnitude > velocity.magnitude)
                    velocity = Vector3.zero;
                else
                    velocity -= deceleration;

            }
            rb.MovePosition(newPos);
        }
    }


    public void Teleport(Vector3 newPosition)
    {
        rb.position = newPosition;
    }

    private void OnDrawGizmos()
    {
        Vector3 boxSize = GetComponent<BoxCollider>().size;
        Vector3 objectScale = transform.lossyScale;
        Gizmos.color = new Color(0, 0, 1, 0.1f);
        Gizmos.DrawCube(GetComponent<Rigidbody>().position, new Vector3(boxSize.x * objectScale.x + skinWidth, boxSize.y * objectScale.y + skinWidth, boxSize.z * objectScale.z + skinWidth));
        Gizmos.color = new Color(0, 1, 0, 0.1f);
        Gizmos.DrawCube(GetComponent<Rigidbody>().position, new Vector3(boxSize.x * objectScale.x + skinWidth, boxSize.y * objectScale.y + skinWidth + 0.4f, boxSize.z * objectScale.z + skinWidth));
    }

}
