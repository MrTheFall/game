using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class Scope : MonoBehaviourPunCallbacks
{
    public Animator animator;
    public GameObject Rifle;
    public GameObject Pistol;
    public GameObject Recoil_Rotation;

    public string RifleBool;
    public string PistolBool;

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;
        if (Recoil_Rotation.transform.childCount > 0)
        {
            if (Recoil_Rotation.transform.GetChild(0).name == Rifle.transform.name)
            {
                if (Input.GetButtonDown("Fire2"))
                {
                    animator.SetBool(RifleBool, true);
                }
                if (Input.GetButtonUp("Fire2"))
                {
                    animator.SetBool(RifleBool, false);
                }
            }
            if (Recoil_Rotation.transform.GetChild(0).name == Pistol.transform.name)
            {
                if (Input.GetButtonDown("Fire2"))
                {
                    animator.SetBool(PistolBool, true);
                }
                if (Input.GetButtonUp("Fire2"))
                {
                    animator.SetBool(PistolBool, false);
                }
            }
        }
    }
}
