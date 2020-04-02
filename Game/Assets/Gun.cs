using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Gun : MonoBehaviour
{

    public GameObject theBullet;
    public Transform barrelEnd;

    public int bulletSpeed;
    public float despawnTime = 3.0f;
    public int maxAmmo = 5;
    public int currentAmmo;
    public float reloadTime = 1f;
    private bool isReloading = false;
    public Text currentAmmoDisplay;

    public bool shootAble = true;
    public float waitBeforeNextShot = 0.4f;

    void Start()
    {
        currentAmmo = maxAmmo;
    }

    void OnEnable()
    {
        isReloading = false;
    }

    void Update()
    {
        currentAmmoDisplay.text = currentAmmo.ToString();

        if (isReloading)        
            return;

        if (currentAmmo <= 0)
        {
            isReloading = true;
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetKey(KeyCode.Mouse0))
        {
            if (shootAble)
            {
                shootAble = false;
                Shoot();
                StartCoroutine(ShootingYield());
            }
        }
    }

    IEnumerator ShootingYield()
    {
        yield return new WaitForSeconds(waitBeforeNextShot);
        shootAble = true;
    }
    void Shoot()
    {
        currentAmmo--;
        Debug.Log(currentAmmo);
        var bullet = Instantiate(theBullet, barrelEnd.position, barrelEnd.rotation);
        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * bulletSpeed;

        Destroy(bullet, despawnTime);
    }

    IEnumerator Reload()
    {
        Debug.Log("Reloading..");
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        isReloading = false;
    }
}