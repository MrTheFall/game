using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using System.Net.Cache;
using Photon.Pun.Demo.Procedural;

public class PlayerWeapon_Pickup : MonoBehaviourPunCallbacks
{
    public Camera cam;
    public float pickupRange;
    public LayerMask pickupLayerMask;
    public Transform Recoil_Rotation;
    public Transform Recoil_Position;
    public Transform Recoil_Camera;
    public GameObject Ground_Check;
    public GameObject weapon;
    private Weapon myWeapon;


    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && photonView.IsMine)
        {
            photonView.RPC("RaycastPickUpGun", RpcTarget.All);
        }

        if (Input.GetKeyDown(KeyCode.G) && photonView.IsMine)
        {
            photonView.RPC("DropWeapon", RpcTarget.All);

        }
    }


    [PunRPC]
    public RaycastHit RaycastPickUpGun()// Returns the RayCast Hit so that other scripts will know what was hit
    {
        if (photonView.IsMine)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Debug.DrawRay(cam.transform.position, cam.transform.forward, Color.red, pickupRange);
            if (Physics.Raycast(ray, out hit, pickupRange, pickupLayerMask) && (Recoil_Rotation.transform.childCount == 0))
            {
                if (!hit.collider.GetComponent<PointGun>().isEquipped)
                {
                    photonView.RPC("ClaimWeapon", RpcTarget.All, hit.collider.GetComponent<PhotonView>().ViewID);
                }
            }
        }
        return new RaycastHit();
    }

    [PunRPC]
    void ClaimWeapon(int id)
    {
        weapon = PhotonView.Find(id).gameObject;
        weapon.GetComponent<PointGun>().isEquipped = true;
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

    [PunRPC]
    public void DropWeapon()
    {
        if (weapon != null) // Checking if we are currently holding a weapon, before we drop it
        {
            weapon.GetComponent<PointGun>().isEquipped = false;
            Vector3 pos = new Vector3(Ground_Check.transform.position.x, Ground_Check.transform.position.y + 0.5f, Ground_Check.transform.position.z);
            weapon.transform.rotation = Ground_Check.transform.rotation;
            weapon.transform.parent = null;
            weapon.transform.position = pos;
            myWeapon.WeaponIsActive(false);
            weapon = null;
        }
    }
}