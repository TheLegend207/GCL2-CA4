using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    //Most of these are pretty self-explainory 
    public Camera playerCamera;
    public float walkSpeed = 6f;
    public float shiftWalkSpeed = 3f;
    public float jumpPower = 7f;
    public float gravity = 10f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f; //How far the player can look up and down
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 1.5f;

    private Vector3 moveDirection = Vector3.zero; //Store player's movements
    private float rotationX = 0; //Store how far the player has looked up and down
    private CharacterController characterController; //Use character controller for height adjustments

    private bool canMove = true; //Ref to allow player to move or not

    void Start()
    {
        characterController = GetComponent<CharacterController>(); //Gets a character controller attached to the object
        Cursor.lockState = CursorLockMode.Locked; //Locks cursor to the middle of the screen
        Cursor.visible = false; //Hide it
    }

    void Update()
    {
        //When changing directions, the key inputs will also change to that direction
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        //For the system to understand how much the player wants to move in each direction
        float curSpeedX = canMove ? walkSpeed * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? walkSpeed * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y; //Temporarily remember jump/fall speed
        moveDirection = (forward * curSpeedX) + (right * curSpeedY); //To allow diagonal movement 
        
        //Hold "left shift" to walk slower
        if (Input.GetKey(KeyCode.LeftShift))
        {
            walkSpeed = shiftWalkSpeed; //Reduce speed
        }
        else
        {
            walkSpeed = 6f; //Regain speed
        }
        //Press "space" to jump
        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection.y = movementDirectionY; //Keep jump speed while in air
        }
        //Apply gravity while in air
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        //Press "left control" to crouch
        if (Input.GetKey(KeyCode.LeftControl) && canMove)
        {
            characterController.height = Mathf.MoveTowards(characterController.height, crouchHeight, crouchSpeed * Time.deltaTime); //Transition to crouch height
            walkSpeed = crouchSpeed; //Reduce speed

        }
        else if (Input.GetKey(KeyCode.LeftShift)) //If the player is holding shift (and not crouching), then adjust to shift walk speed instead
        {
            walkSpeed = shiftWalkSpeed;
        }
        else
        {
            characterController.height = Mathf.MoveTowards(characterController.height, defaultHeight, 6f * Time.deltaTime); //Transition to normal height
            walkSpeed = 6f; //Regain speed
        }
        
        characterController.Move(moveDirection * Time.deltaTime); //Moves the player using character controller
        //Allow player to look around when controls are enabled
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed; //Update camera's verticle rotation based on the mouse up/down movement
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0); //Apply verticle rotation to camera only
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit); //Stop player from looking too far up/down
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0); //Rotate the player
        }

        
    }
}
