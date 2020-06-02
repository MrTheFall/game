using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Weapon_Pickup : MonoBehaviourPunCallbacks
{
    private Weapon_Holder holder;
    private Weapon myWeapon;
    private bool canPickUp = true;
    private float waitBeforePickUp = 1f;

    private void Start()
    {
        canPickUp = true;
        myWeapon = GetComponent<Weapon>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PickupWeapon();
        }
    }

    private void PickupWeapon()
    {
        if (canPickUp && (GameObject.Find("Recoil_Rotation").transform.childCount == 0))
        {
            Weapon_Holder holder = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<Weapon_Holder>();
            holder.photonView.RPC("PickupWeapon", RpcTarget.All, gameObject);
            myWeapon.HandleUI();
            myWeapon.WeaponIsActive(true);
            holder.hand.gameObject.GetComponent<Sway>().enabled = photonView.IsMine;
        }
    }
    public void DropWeapon()
    {
        GameObject obj = GameObject.Find("GroundCheck");
        Vector3 pos = new Vector3(obj.transform.position.x, obj.transform.position.y + 0.5f, obj.transform.position.z);
        transform.rotation = obj.transform.rotation;
        transform.parent = null;
        transform.position = pos;
        myWeapon.WeaponIsActive(false);
        StartCoroutine(PickUpTimer());
    }

    IEnumerator PickUpTimer()
    {
        canPickUp = false;
        yield return new WaitForSeconds(waitBeforePickUp);
        canPickUp = true;
    }
}
