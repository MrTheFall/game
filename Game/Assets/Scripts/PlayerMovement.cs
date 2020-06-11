using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class PlayerMovement : MonoBehaviourPunCallbacks
{
    public CharacterController controller;
    public AudioSource audio;
    public AudioClip clip;

    public float speed = 7f;
    public float normalSpeed = 7f;
    public float sprintSpeed = 10f;
    public bool isSprinting = false;
    public bool isCrouching = false;
    public bool isWalking = false;
    public bool isCrouchingWalking = false;

    public float gravity = -39.24f;
    public float jumpHeight = 3;

    public Transform groundCheck;
    public float groundDistance = 0.1f;

    public Animator animator;

    Vector3 velocity;
    bool isGrounded;

    // Start is called before the first frame update
    void Start()
    {
        audio = gameObject.transform.GetComponent<AudioSource>();
        if (!photonView.IsMine)
        {
            audio.volume = 0.5f;
            ChangeLayersRecursively(gameObject.transform, "PlayerEnemy");
            gameObject.GetComponent<CharacterController>().enabled = false; // this removes character controller's hitbox, so we can make our own
            Destroy(gameObject.GetComponent<AudioListener>());
        }
        else
        {
            audio.volume = 0.2f;
            speed = normalSpeed;
            gameObject.transform.Find("Default Armless Player").gameObject.SetActive(false);
            gameObject.transform.Find("Default Arms").gameObject.SetActive(false);
        }
    }
    public static void ChangeLayersRecursively(Transform trans, string name)
    {
        if (trans.gameObject.layer != LayerMask.NameToLayer("Weapon"))
        {
            trans.gameObject.layer = LayerMask.NameToLayer(name);
            foreach (Transform child in trans)
            {
                ChangeLayersRecursively(child, name);
            }
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {

            bool pause = Input.GetKeyDown(KeyCode.Escape);

            if (pause)
            {
                GameObject.Find("Pause").GetComponent<Pause>().TogglePause();
            }

            if (Pause.paused) return;


            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance);

            if (isGrounded && velocity.y < 0)
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

            isSprinting = Input.GetKey(KeyCode.LeftShift) && Input.GetAxisRaw("Vertical") > 0 && !Input.GetMouseButton(1) && !Input.GetMouseButton(0) && !isCrouching;
            if (isSprinting)
            {
                speed = sprintSpeed;
            }
            else speed = normalSpeed;

            isCrouching = Input.GetKey(KeyCode.LeftControl);
            if (isCrouching)
            {
                animator.SetBool("Crouching", true);
                speed = normalSpeed / 2;
            }
            else
            {
                animator.SetBool("Crouching", false);
                speed = normalSpeed;
            }

            isWalking = Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Horizontal") != 0;
            if (isWalking)
            {
                animator.SetBool("Walking", true);
            }
            else animator.SetBool("Walking", false);

            isCrouchingWalking = isWalking && isCrouching;
            if (isCrouchingWalking)
            {
                animator.SetBool("CrouchingWalk", true);
            }
            else animator.SetBool("CrouchingWalk", false);

            if (isWalking && isGrounded && !audio.isPlaying)
            {
                photonView.RPC("Step", RpcTarget.All);
            }

            }
    }

    [PunRPC]
    public void Step()
    {
        audio.PlayOneShot(clip);
    }
}
