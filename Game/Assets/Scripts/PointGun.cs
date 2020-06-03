using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class PointGun : Weapon// Inherit everything from the "Weapon" script
{
    [Header("Textures")]
    public GameObject bulletholePrefab;

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
                    GameObject newHole = Instantiate(bulletholePrefab, hit.point + hit.normal * 0.001f, Quaternion.identity) as GameObject;
                    newHole.transform.LookAt(hit.point + hit.normal);
                    Destroy(newHole, 5f);

                    photonView.RPC("spawnBulletHole", RpcTarget.All, hit.point, hit.normal, Quaternion.identity);

                    if (gameObject.transform.root.GetComponent<PhotonView>().IsMine)
                    {
                        //if we hit enemy player
                        if (hit.collider.gameObject.layer == 11)
                        {
                            //RPC with command to damage enemy player
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
}
