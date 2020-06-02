using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

public class PlayerWeapon_Pickup : MonoBehaviourPunCallbacks
{
    public Camera cam;
    public float pickupRange;
    public LayerMask pickupLayerMask;
    public Transform Recoil_Rotation;
    public Transform Recoil_Position;
    public Transform Recoil_Camera;
    public GameObject weapon;
    private Weapon myWeapon;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        
            photonView.RPC("RaycastPickUpGun", RpcTarget.All);
    }


    [PunRPC]
    public RaycastHit RaycastPickUpGun()// Returns the RayCast Hit so that other scripts will know what was hit
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Debug.DrawRay(cam.transform.position, cam.transform.forward, Color.red, pickupRange);
        if (Physics.Raycast(ray, out hit, pickupRange, pickupLayerMask) && (Recoil_Rotation.transform.childCount == 0))
        {
            weapon = hit.transform.gameObject;
            weapon.transform.SetParent(Recoil_Rotation);
            weapon.transform.position = Recoil_Rotation.transform.position;
            weapon.transform.rotation = Recoil_Rotation.transform.rotation;
            myWeapon = weapon.GetComponent<Weapon>();
            myWeapon.HandleUI();
            myWeapon.WeaponIsActive(true);
            weapon.GetComponent<Weapon>().RecoilPositionTransform = Recoil_Position;
            weapon.GetComponent<PointGun>().RecoilPositionTransform = Recoil_Position;
            weapon.GetComponent<Weapon>().RecoilRotationTransform = Recoil_Rotation;
            weapon.GetComponent<PointGun>().RecoilRotationTransform = Recoil_Rotation;
            weapon.GetComponent<Weapon>().RecoilCamTransform = Recoil_Camera;
            weapon.GetComponent<PointGun>().RecoilCamTransform = Recoil_Camera;
            weapon.GetComponent<Weapon>().cam = cam;
            weapon.GetComponent<PointGun>().cam = cam;
        }
        return hit;
    }
}