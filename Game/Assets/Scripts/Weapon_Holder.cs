using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class Weapon_Holder : MonoBehaviourPunCallbacks
{
    public Transform hand;
    public GameObject weapon;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            DropWeapon();
        }
    }

    [PunRPC]
    public void PickupWeapon(GameObject _weapon)
    {
        if (!photonView.IsMine) return;
        hand = gameObject.transform;
        weapon = _weapon;
        weapon.transform.SetParent(hand);
        weapon.transform.position = hand.position;
        weapon.transform.rotation = hand.rotation;
    }

    public void DropWeapon()
    {
        if (weapon != null) // Checking if we are currently holding a weapon, before we drop it
        {
            weapon.GetComponent<Weapon_Pickup>().DropWeapon();
            weapon = null;
        }
    }
}