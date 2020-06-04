using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerMovement : MonoBehaviourPunCallbacks
{
    public CharacterController controller;

    public float speed = 15f;
    public float gravity = -39.24f;
    public float jumpHeight = 3;


    public Transform groundCheck;
    public float groundDistance = 0.1f;
    

    Vector3 velocity;
    bool isGrounded;

    // Start is called before the first frame update
    void Start()
    {
        if (!photonView.IsMine) gameObject.layer = 11;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;


        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance);

        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);


        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        }
        

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}
