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
            ammoType.totalAmmo += ammoAmount;
            Destroy(gameObject);
        }
    }
}
