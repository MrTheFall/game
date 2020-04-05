using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointGun : Weapon// Inherit everything from the "Weapon" script
{
    public override void Shoot()// Overriding the Shoot() method from the "Weapon" script
    {
        if (!isReloading)
        {
            base.Shoot();// Call the base method so the UI/Ammo will update

            RaycastHit hit = GetHitData();// Get the Raycast Hit from the GetHitData() method in the Weapon Script

            if (hit.collider != null && hit.collider.CompareTag("Enemy"))// If we actually hit something and it was tagged as "Enemy"
            {
                hit.collider.GetComponent<Enemy>().TakeDamage(damage);// Get the enemy script, and call the Take Damage method
            }
        }
    }

}
