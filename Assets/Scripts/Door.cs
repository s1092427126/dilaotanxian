using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    Animator anim;
    int openID;

    private void Start()
    {
        anim = GetComponent<Animator>();

        openID = Animator.StringToHash("Open");

        GameManager.RegisterDoor(this);
    }

    public void open()
    {
        anim.SetTrigger(openID);
        AudioManager.PlayDoorOpenAudio();
    }

}
