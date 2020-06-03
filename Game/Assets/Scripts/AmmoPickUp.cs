using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPickUp : MonoBehaviour
{
    public Ammo ammoType;
    public int ammoAmount;
     private void OnTriggerEnter(Collider other)
    {   
        if(other.tag == "Player")
        {
            if (other.transform.Find("Camera Holder/Hand/Recoil_Position/Recoil_Rotation").childCount != 0)   
            {
                if (other.transform.root.GetComponentInChildren<PointGun>().ammoType.name == ammoType.name)
                {
                    other.gameObject.transform.root.GetComponentInChildren<PointGun>().ammoReserve += ammoAmount;
                    Destroy(gameObject);
                }
            }
        }
    }
}

