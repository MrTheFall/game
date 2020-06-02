using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class Scope : MonoBehaviourPunCallbacks
{
    public Animator animator;
    public string RifleName = "Rifle";
    public string PistolName = "Pistol";
    public GameObject Recoil_Rotation;

    public string RifleBool = "RifleZoomedIn";
    public string PistolBool = "PistolZoomedIn";

    // Update is called once per frame
    private void Start()
    {
        RifleBool = "PistolZoomedIn";
        PistolBool = "PistolZoomedIn";
    }
    void Update()
    {
        if (!photonView.IsMine) return;
        if (Recoil_Rotation.transform.childCount > 0)
        {
            if (Recoil_Rotation.transform.GetChild(0).name == RifleName)
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
            if (Recoil_Rotation.transform.GetChild(0).name == PistolName)
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
