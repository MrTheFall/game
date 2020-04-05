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
    public int totalAmmo;// Total amount of ammo you can have for this gun
    public float reloadTime; // Must be changed for every new gun
    public bool isReloading = false;

    public float damage;
    public float range;

    public float fireCoolDown;// How long should this weapon wait before it can shoot again
    private float timer = 0f;

    [Header("UI")]
    public Sprite customCrossHair;// A sprite that represents the crosshair for this particular weapon

    [Header("Raycast Camera")]
    public Camera cam;// The camera used for raycasting

    private Weapon_UI ui;

    private AudioSource myAudio;

    private bool isActive = false;


    private void Start()
    {
        isReloading = false;
        ui = FindObjectOfType<Weapon_UI>();
        myAudio = GetComponent<AudioSource>();
        myAudio.playOnAwake = false;
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
            if(timer>=fireCoolDown && ammo >= 1)
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
        ui.Update_UI(ammo, totalAmmo);
    }

    IEnumerator Reload(float reloadTime)
    {
        if (!isReloading)
        {
            if (totalAmmo > 0 && ammo != ammoCanHold)
            {
                isReloading = true;
                yield return new WaitForSeconds(reloadTime);
                int ammoNeeded = ammoCanHold - ammo;

                if (ammoNeeded <= totalAmmo)
                {
                    ammo += ammoNeeded;
                    totalAmmo -= ammoNeeded;
                }
                else if (totalAmmo == 0)
                { }
                else
                {
                    ammo += totalAmmo;
                    totalAmmo = 0;
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
            myAudio.Play();
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
