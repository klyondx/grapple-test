using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController2D controller;
    public float runSpeed = 40f;
    public Animator animator;

    float horizontalMove = 0f;
    bool jump = false;
    float runAnimationThreshold = 1f;
    bool isJumping = false;

    void Start()
    {
        
    }

    void Update()
    {
        float horizontalRaw = Input.GetAxisRaw("Horizontal");
        horizontalMove = horizontalRaw * runSpeed;
        animator.SetFloat("Speed", Mathf.Abs(horizontalRaw) > 0.5 && !isJumping ? runAnimationThreshold : 0);

        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
        }
     
    }

    private void FixedUpdate()
    {
        controller.Move(horizontalMove * Time.fixedDeltaTime, false, jump);
        jump = false;
    }

    private void setIsJumping(bool isJumping)
    {
        this.isJumping = isJumping;
    }

    public void inAir()
    {
        setIsJumping(true);
    }

    public void onGround()
    {
        setIsJumping(false);
    }
}
