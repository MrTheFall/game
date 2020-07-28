using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class Scope : MonoBehaviourPunCallbacks
{
    public Animator animator;
    public string RifleName;
    public string PistolName;
    public string DeagleName;
    public string M4A1Name;
    public string AWPName;
    public GameObject Recoil_Rotation;
    public GameObject scopeOverlay;
    public Camera mainCamera;
    public GameObject Gun;
    public float scopedFOV = 25;
    public float normalFOV;

    public string ReloadingBool = "Reloading";
    public string RifleBool = "RifleZoomedIn";
    public string PistolBool = "PistolZoomedIn";
    public string DeagleBool = "DeagleZoomedIn";
    public string M4A1Bool = "M4A1ZoomedIn";
    public string AWPBool = "AWPZoomedIn";

    // Update is called once per frame
    private void Start()
    {
        mainCamera = gameObject.transform.root.Find("Camera Holder/Recoil Camera/Main Camera").GetComponent<Camera>();
        normalFOV = mainCamera.fieldOfView;
        scopeOverlay = GameObject.Find("HUD/Scope/ScopeOverlay");
        scopeOverlay.SetActive(false);
        RifleBool = "PistolZoomedIn";
        PistolBool = "PistolZoomedIn";
        DeagleBool = "DeagleZoomedIn";
    }
    void Update()
    {
        if (!photonView.IsMine) return;
        if (Recoil_Rotation.transform.childCount > 0)
        {
            if (Recoil_Rotation.transform.GetChild(0).GetComponent<PointGun>().isReloading == true)
            {
                animator.SetBool(ReloadingBool, true);
            }
            else animator.SetBool(ReloadingBool, false);

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
            if (Recoil_Rotation.transform.GetChild(0).name == DeagleName)
            {
                if (Input.GetButtonDown("Fire2"))
                {
                    animator.SetBool(DeagleBool, true);
                }
                if (Input.GetButtonUp("Fire2"))
                {
                    animator.SetBool(DeagleBool, false);
                }
            }
            if (Recoil_Rotation.transform.GetChild(0).name == M4A1Name)
            {
                if (Input.GetButtonDown("Fire2"))
                {
                    animator.SetBool(M4A1Bool, true);
                }
                if (Input.GetButtonUp("Fire2"))
                {
                    animator.SetBool(M4A1Bool, false);
                }
            }
            if (Recoil_Rotation.transform.GetChild(0).name == AWPName)
            {
                if (Input.GetButtonDown("Fire2"))
                {
                    Gun = Recoil_Rotation.transform.GetChild(0).Find("Gun").gameObject;
                    animator.SetBool(AWPBool, true);
                    StartCoroutine(OnScoped());

                }
                if (Input.GetButtonUp("Fire2"))
                {
                    OnUnscoped();
                    animator.SetBool(AWPBool, false);
                }
            }
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetAxisRaw("Vertical") > 0 && !Input.GetMouseButton(1) && !Input.GetMouseButton(0) && !Input.GetKey(KeyCode.LeftControl))
        {
            animator.SetBool("isRunning", true);
        }
        else animator.SetBool("isRunning", false);
    }

    IEnumerator OnScoped()
    {
        yield return new WaitForSeconds(0.25f);
        Gun.SetActive(false);
        scopeOverlay.SetActive(true);
        mainCamera.fieldOfView = scopedFOV;
        if(animator.GetBool(AWPBool) == false)
        {
            OnUnscoped();
        }
    }
    void OnUnscoped()
    {
        Gun.SetActive(true);
        scopeOverlay.SetActive(false);
        mainCamera.fieldOfView = normalFOV;
    }
}
