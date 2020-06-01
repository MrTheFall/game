using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Weapon_Pickup))]
public abstract class Weapon : MonoBehaviour// An abstract class. 
{
    [Header("General Info")]
    public int ammo;// How much is currently in the weapon
    public int ammoCanHold;// How much can this weapon hold at a time (max amount)
    public float reloadTime; // Must be changed for every new gun
    public bool isReloading = false;
    public Ammo ammoType;

    public float damage;
    public float range;

    public float fireCoolDown;// How long should this weapon wait before it can shoot again
    private float timer = 0f;

    [Header("Sound")]
    public AudioSource shootAudio;
    public AudioSource reloadAudio;

    [Header("UI")]
    public Sprite customCrossHair;// A sprite that represents the crosshair for this particular weapon

    [Header("Raycast Camera")]
    public Camera cam;// The camera used for raycasting

    private Weapon_UI ui;

    private bool isActive = false;

    private void Start()
    {
        isReloading = false;
        ui = FindObjectOfType<Weapon_UI>();
    }

    private void Update()
    {
        if (!isActive)// Need this here. Otherwise every weapon in your game will fire when you press the LMB. 
        {
            return;
        }

        timer += Time.deltaTime;

        if (Input.GetMouseButton(0))
        {
            if(timer>=fireCoolDown && ammo > 0)
            {
                Shoot();
                timer = 0f;
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(Reload(reloadTime));
            return;
        }
    }

    public void WeaponIsActive(bool _value)
    {
        isActive = _value;

        if (isActive)
        {
            ui.SetCrossHair(customCrossHair);
        }
        else
        {
            ui.HideUI();
        }
    }

    public void HandleUI()
    {
        ui.Update_UI(ammo, ammoType.totalAmmo);
    }

    IEnumerator Reload(float reloadTime)
    {
        if (!isReloading)
        {
            if (ammoType.totalAmmo > 0 && ammo != ammoCanHold)
            {
                reloadAudio.Play();
                isReloading = true;
                yield return new WaitForSeconds(reloadTime);
                int ammoNeeded = ammoCanHold - ammo;

                if (ammoNeeded <= ammoType.totalAmmo)
                {
                    ammo += ammoNeeded;
                    ammoType.totalAmmo -= ammoNeeded;
                }
                else if (ammoType.totalAmmo == 0)
                { }
                else
                {
                    ammo += ammoType.totalAmmo;
                    ammoType.totalAmmo = 0;
                }
            }
            HandleUI();
            // Play reloading animation here
            isReloading = false;
        }
    }

    public virtual void Shoot()
    {
        if (!isReloading)
        {
            shootAudio.Play();
            ammo -= 1;
            HandleUI();
            // Play shooting animation here
        }
    }

    public RaycastHit GetHitData()// Returns the RayCast Hit so that other scripts will know what was hit
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, range))
        {
            return hit;
        }

        return hit;
    }

}
