using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class PointGun : Weapon// Inherit everything from the "Weapon" script
{
    [Header("Textures")]
    public GameObject bulletholePrefab;

    private void Awake()
    {
        if(gameObject.transform.Find("Default Arms") != null) HandsDisable();
    }

    [PunRPC]
    public override void Shoot()// Overriding the Shoot() method from the "Weapon" script
    {
        if (!isReloading)
        {
            base.Shoot();// Call the base method so the UI/Ammo will update

            RaycastHit hit = GetHitData();// Get the Raycast Hit from the GetHitData() method in the Weapon Script

            if ((hit.collider != null) && (hit.transform.gameObject.layer != LayerMask.NameToLayer("Player")) && hit.transform.gameObject.layer != LayerMask.NameToLayer("Weapon"))
            {


                if (gameObject.transform.root.GetComponent<PhotonView>().IsMine)
                {
                    if (hit.transform.gameObject.layer != 11 && hit.transform.gameObject.layer != 12)
                    {
                        photonView.RPC("spawnBulletHole", RpcTarget.All, hit.point, hit.normal, Quaternion.identity);
                        GameObject newHole = Instantiate(bulletholePrefab, hit.point + hit.normal * 0.001f, Quaternion.identity) as GameObject;
                        newHole.transform.LookAt(hit.point + hit.normal);
                        Destroy(newHole, 5f);
                    }


                    if (gameObject.transform.root.GetComponent<PhotonView>().IsMine)
                    {
                        //if we hit enemy player
                        if (hit.collider.gameObject.layer == 11)
                        {
                            hit.collider.transform.root.gameObject.GetPhotonView().RPC("TakeDamageRPC", RpcTarget.All, damage, PhotonNetwork.LocalPlayer.ActorNumber);
                        }
                    }
                }
            }
        }
    }

    [PunRPC]
    public void spawnBulletHole(Vector3 hitPoint, Vector3 hitNormal, Quaternion identity)
    {
        GameObject newHole = Instantiate(bulletholePrefab, hitPoint + hitNormal * 0.001f, identity) as GameObject;
        newHole.transform.LookAt(hitPoint + hitNormal);
        Destroy(newHole, 5f);
    }

    public void HandsEnable()
    {
        gameObject.transform.Find("Default Arms").gameObject.SetActive(true);
    }

    public void HandsDisable()
    {
        gameObject.transform.Find("Default Arms").gameObject.SetActive(false);
    }

}
