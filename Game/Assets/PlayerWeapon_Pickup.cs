using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using System.Net.Cache;
using Photon.Pun.Demo.Procedural;
using System.Drawing;

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
    private PointGun myWeapon;

    [Space]
    public GameObject StandartWeapon;

    // Start is called before the first frame update
    private void Start()
    {
        GameObject t_weapon = (GameObject)PhotonNetwork.Instantiate("Weapon/" + StandartWeapon.name + "/" + StandartWeapon.name, gameObject.transform.position, gameObject.transform.rotation);
        t_weapon.name = StandartWeapon.name;
        photonView.RPC("ClaimWeapon", RpcTarget.All, t_weapon.GetPhotonView().ViewID);
    }


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
            Debug.DrawRay(cam.transform.position, cam.transform.forward, UnityEngine.Color.red, pickupRange);
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
        gameObject.transform.Find("Default Arms").gameObject.SetActive(false);
        weapon = PhotonView.Find(id).gameObject.transform.root.gameObject;
        weapon.GetComponent<PointGun>().isEquipped = true;
        weapon.transform.SetParent(Recoil_Rotation);
        weapon.transform.position = Recoil_Rotation.transform.position;
        weapon.transform.rotation = Recoil_Rotation.transform.rotation;
        myWeapon = weapon.GetComponent<PointGun>();
        myWeapon.ui = FindObjectOfType<Weapon_UI>();
        myWeapon.HandleUI();
        myWeapon.RecoilPositionTransform = Recoil_Position ;
        myWeapon.RecoilRotationTransform = Recoil_Rotation;
        myWeapon.WeaponIsActive(true);
        myWeapon.RecoilCamTransform = Recoil_Camera;
        myWeapon.cam = cam;
        myWeapon.HandsEnable();
    }

    [PunRPC]
    public void DropWeapon()
    {
        if (weapon != null) // Checking if we are currently holding a weapon, before we drop it
        {
            weapon.GetComponent<PointGun>().isEquipped = false;
            weapon.GetComponent<PointGun>().HandsDisable();
            gameObject.transform.Find("Default Arms").gameObject.SetActive(true);
            Vector3 pos = new Vector3(Ground_Check.transform.position.x, Ground_Check.transform.position.y + 1f, Ground_Check.transform.position.z);
            weapon.transform.rotation = Ground_Check.transform.rotation;
            weapon.transform.parent = null;
            weapon.transform.position = pos;
            myWeapon.WeaponIsActive(false);
            weapon = null;
            if(photonView.IsMine) gameObject.transform.Find("Default Arms").gameObject.SetActive(false);
        }
    }
}