﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rig;
    private BoxCollider2D bcoll;

    [Header("移动参数")]
    public float Speed = 8f;
    public float CrouchSpeed = 3f;

    public float xVelocity;

    [Header("跳跃参数")]
    public float JumpForce = 6.3f;
    public float JumpHoldForce = 1.9f;
    public float JumpHoldDuration = 0.1f;
    public float CrouchJumpBoost = 2.5f;
    public float HangingJumpForce = 15;

    float jumpTime;

    [Header("状态")]
    public bool isCrouch;
    public bool isOnGround;
    public bool isJump;
    public bool isHeadBlocked;
    public bool isHanging;

    [Header("环境检测")]
    public float footOffset = 0.4f;
    public float headClearance = 0.5f;
    public float groundDistance = 0.2f;
    float playerHeight;
    public float eyeHeight = 1.5f;
    public float grabDistance = 0.4f;
    public float reachOffset = 0.7f;

    public LayerMask groundLayer;

    //按键设置
    bool jumpPressed;
    bool jumpHeld;
    bool crouchHeld;
    bool crouchPressed;
    


    //碰撞体数据
    Vector2 CollStandSize;
    Vector2 CollStandOffset;
    Vector2 CollCrouchSize;
    Vector2 CollCrouchOffset;


    void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        bcoll = GetComponent<BoxCollider2D>();

        playerHeight = bcoll.size.y;

        CollStandSize = bcoll.size;
        CollStandOffset = bcoll.offset;
        CollCrouchSize = new Vector2(bcoll.size.x, bcoll.size.y / 2f);
        CollCrouchOffset = new Vector2(bcoll.offset.x, bcoll.offset.y / 2f);
    }

    // Update is called once per frame
    void Update()
    {
        if(GameManager.GameOver())
        {
            return;
        }
        jumpPressed = Input.GetButtonDown("Jump");
        jumpHeld = Input.GetButton("Jump");
        crouchHeld = Input.GetButton("Crouch");
        crouchPressed = Input.GetButtonDown("Crouch");
    }

    private void FixedUpdate()
    {
        if (GameManager.GameOver())
        {
            return;
        }
        PhysicsCcheck();
        GroundMovent();
        jump();
        
    }

    void PhysicsCcheck()
    {
        RaycastHit2D leftCheck = Raycast(new Vector2(-footOffset, 0f), Vector2.down, groundDistance, groundLayer);
        RaycastHit2D rightCheck = Raycast(new Vector2(footOffset, 0f), Vector2.down, groundDistance, groundLayer);

        if (leftCheck || rightCheck) 
        {
            isOnGround = true;
        }
        else
        {
            isOnGround = false;
        }

        RaycastHit2D headCheck = Raycast(new Vector2(0f, bcoll.size.y), Vector2.up, headClearance, groundLayer);

        if (headCheck)
        {
            isHeadBlocked = true;
        }
        else
        {
            isHeadBlocked = false;
        }

        float direction = transform.localScale.x;
        Vector2 grabDir = new Vector2(direction, 0f);

        RaycastHit2D blockedCheck = Raycast(new Vector2(footOffset * direction, playerHeight), grabDir, grabDistance, groundLayer);
        RaycastHit2D wellCheck = Raycast(new Vector2(footOffset * direction, eyeHeight), grabDir, grabDistance, groundLayer);
        RaycastHit2D ledgeCheck = Raycast(new Vector2(reachOffset * direction, playerHeight), Vector2.down, grabDistance, groundLayer);

        if(!isOnGround && rig.velocity.y < 0f && ledgeCheck && wellCheck && !blockedCheck)
        {
            Vector3 pos = transform.position;

            pos.x += (wellCheck.distance - 0.05f) * direction;

            pos.y -= ledgeCheck.distance;

            transform.position = pos;

            rig.bodyType = RigidbodyType2D.Static;
            isHanging = true;
        }
    }

    void GroundMovent()
    {
        if (isHanging)
            return;
        if(crouchHeld && !isCrouch && isOnGround)
        {
            Crouch();
        }
        else if(!crouchHeld && isCrouch && !isHeadBlocked)
        {
            StandUp();
        }
        else if(!isOnGround && isCrouch)
        {
            StandUp();
        }
        xVelocity = Input.GetAxis("Horizontal");
        if (isCrouch)
        {
            xVelocity /= CrouchSpeed;
        }
        rig.velocity = new Vector2(xVelocity * Speed, rig.velocity.y);
        Face();
    }

    //跳跃
    void jump()
    {
        if (isHanging)
        {
            if (jumpPressed)
            {
                rig.bodyType = RigidbodyType2D.Dynamic;
                rig.velocity = new Vector2(rig.velocity.x, HangingJumpForce);
                isHanging = false;
            }
            if (crouchPressed)
            {
                rig.bodyType = RigidbodyType2D.Dynamic;
                isHanging = false;
            }
        }
        if(jumpPressed && isOnGround && !isJump && !isHeadBlocked)
        {
            if (isCrouch)
            {
                StandUp();
                rig.AddForce(new Vector2(0f, CrouchJumpBoost), ForceMode2D.Impulse);
            }
            isOnGround = false;
            isJump = true;

            jumpTime = Time.time + JumpHoldDuration;

            rig.AddForce(new Vector2(0f, JumpForce), ForceMode2D.Impulse);

            AudioManager.PlayJumpAudio();
        }
        else if (isJump)
        {
            if (jumpHeld)
            {
                rig.AddForce(new Vector2(0f, JumpHoldForce), ForceMode2D.Impulse);
            }
            if (jumpTime < Time.time)
            {
                isJump = false;
            }
        }
    }


    //脸部朝向
    void Face()
    {
        if (xVelocity > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        if (xVelocity < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    //下蹲
    void Crouch()
    {
        isCrouch = true;
        bcoll.size = CollCrouchSize;
        bcoll.offset = CollCrouchOffset;
    }

    //站起
    void StandUp()
    {
        isCrouch = false;
        bcoll.size = CollStandSize;
        bcoll.offset = CollStandOffset;
    }

    RaycastHit2D Raycast(Vector2 offset,Vector2 rayDiraction,float length,LayerMask layer)
    {
        Vector2 pos = transform.position;

        RaycastHit2D hit = Physics2D.Raycast(pos + offset, rayDiraction, length, layer);

        Color color = hit ? Color.red : Color.green;

        Debug.DrawRay(pos + offset, rayDiraction * length, color);

        return hit;
    }
}
