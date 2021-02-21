using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    Animator anim;
    PlayerMovement movement;
    Rigidbody2D rig;

    int speedID;
    int crouchID;
    int hangingID;
    int groundID;
    int fallID;

    void Start()
    {
        anim = GetComponent<Animator>();
        movement = GetComponentInParent<PlayerMovement>();
        rig = GetComponentInParent<Rigidbody2D>();

        speedID = Animator.StringToHash("speed");
        groundID = Animator.StringToHash("isOnGround");
        crouchID = Animator.StringToHash("isCrouching");
        hangingID = Animator.StringToHash("isHanging");
        fallID = Animator.StringToHash("verticalVelocity");
    }

    void Update()
    {
        anim.SetFloat(speedID, Mathf.Abs(movement.xVelocity));
        anim.SetBool(groundID, movement.isOnGround);
        anim.SetBool(crouchID, movement.isCrouch);
        anim.SetBool(hangingID, movement.isHanging);
        anim.SetFloat(fallID, rig.velocity.y);
    }

    public void StepAudio()
    {
        AudioManager.PlayFootstepAudio();
    }

    public void CrouchStepAudio()
    {
        AudioManager.PlayCrouchFootstepAudio();
    }
}
