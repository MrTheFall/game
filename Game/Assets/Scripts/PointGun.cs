using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PointGun : MonoBehaviourPunCallbacks// Inherit everything from the "Weapon" script
{
    [Header("Textures")]
    public GameObject bulletholePrefab;

    [Header("General Info")]
    public int ammo;// How much is currently in the weapon
    public int ammoCanHold;// How much can this weapon hold at a time (max amount)
    public int ammoReserve;
    public float reloadTime; // Must be changed for every new gun
    public bool isReloading = false;
    public Ammo ammoType;

    public int damage;
    public float range;

    public float fireCoolDown;// How long should this weapon wait before it can shoot again
    private float timer = 0f;

    [Header("Recoil_Transform")]
    public Transform RecoilPositionTransform;
    public Transform RecoilRotationTransform;
    [Space(10)]
    [Header("Recoil_Settings")]
    public float PositionDampTime = 6;
    public float RotationDampTime = 9;
    [Space(10)]
    public float Recoil1 = 35;
    public float Recoil2 = 50;
    public float Recoil3 = 35;
    public float Recoil4 = 50;
    [Space(10)]
    public Vector3 RecoilRotation = new Vector3(-10, 4, 6);
    public Vector3 RecoilKickBack = new Vector3(0.015f, 0.01f, -0.075f);

    public Vector3 RecoilRotation_Aim;
    public Vector3 RecoilKickBack_Aim;
    [Space(10)]
    public Vector3 CurrentRecoil1;
    public Vector3 CurrentRecoil2;
    public Vector3 CurrentRecoil3;
    public Vector3 CurrentRecoil4;
    [Space(10)]
    public Vector3 RotationOutput;

    [Header("Cam Recoil")]
    public Transform RecoilCamTransform;
    public float camRotationSpeed = 6;
    public float camReturnSpeed = 25;

    [Header("Hipfire")]
    public Vector3 camRecoilRotation = new Vector3(5f, 5f, 5f);

    [Header("Aiming")]
    public Vector3 camRecoilRotationAiming = new Vector3(0.5f, 0.5f, 1.5f);

    [Header("State")]
    public bool isAimed;

    private Vector3 camCurrentRotation;
    private Vector3 camRot;

    [Header("Sound")]
    public AudioSource shootAudio;
    public AudioSource reloadAudio;

    [Header("UI")]
    public Sprite customCrossHair;// A sprite that represents the crosshair for this particular weapon

    [Header("Raycast Camera")]
    public Camera cam;// The camera used for raycasting

    [Header("Checks")]
    public bool isEquipped = false;

    public Weapon_UI ui;
    private bool isActive = false;
    private LayerMask bulletGoThrough = ~((1 << 10) | (1 << 8));
    public LayerMask canSpawnBulletHole = ~((1 << 8) | (1 << 11));

    private void Awake()
    {
        if (gameObject.transform.Find("Default Arms") != null) HandsDisable();
    }

    public void Start()
    {
        isReloading = false;
        ui = FindObjectOfType<Weapon_UI>();
    }

    private void Update()
    {
        if (!isEquipped)
        {
            DropRotation();
        }

        if (!gameObject.transform.root.GetComponent<PhotonView>().IsMine) return;

        if (Pause.paused) return;

        if (!isActive)// Need this here. Otherwise every weapon in your game will fire when you press the LMB. 
        {
            return;
        }

        timer += Time.deltaTime;
        if (Input.GetMouseButton(0))
        {
            if (timer >= fireCoolDown && ammo > 0)
            {
                photonView.RPC("Shoot", RpcTarget.All);
                timer = 0f;
            }
        }

        if (Input.GetMouseButton(1))
        {
            isAimed = true;
        }
        else
        {
            isAimed = false;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            photonView.RPC("StartReloadCoroutine", RpcTarget.All);
            return;
        }

        CurrentRecoil1 = Vector3.Lerp(CurrentRecoil1, Vector3.zero, Recoil1 * Time.deltaTime);
        CurrentRecoil2 = Vector3.Lerp(CurrentRecoil2, CurrentRecoil1, Recoil2 * Time.deltaTime);
        CurrentRecoil3 = Vector3.Lerp(CurrentRecoil3, Vector3.zero, Recoil3 * Time.deltaTime);
        CurrentRecoil4 = Vector3.Lerp(CurrentRecoil4, CurrentRecoil3, Recoil4 * Time.deltaTime);

        RecoilPositionTransform.localPosition = Vector3.Slerp(RecoilPositionTransform.localPosition, CurrentRecoil3, PositionDampTime * Time.fixedDeltaTime);
        RotationOutput = Vector3.Slerp(RotationOutput, CurrentRecoil1, RotationDampTime * Time.fixedDeltaTime);
        RecoilRotationTransform.localRotation = Quaternion.Euler(RotationOutput);


        camCurrentRotation = Vector3.Lerp(camCurrentRotation, Vector3.zero, camReturnSpeed * Time.deltaTime);
        camRot = Vector3.Slerp(camRot, camCurrentRotation, camRotationSpeed * Time.fixedDeltaTime);
        RecoilCamTransform.localRotation = Quaternion.Euler(camRot);
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
        if (!gameObject.transform.root.GetComponent<PhotonView>().IsMine) return;
        ui.Update_UI(ammo, ammoReserve);
    }

    [PunRPC]
    public void StartReloadCoroutine()
    {
        StartCoroutine(Reload(reloadTime));
    }

    [PunRPC]
    IEnumerator Reload(float reloadTime)
    {
        if (!isReloading)
        {
            if (!(Input.GetKey(KeyCode.LeftShift) && Input.GetAxisRaw("Vertical") > 0))
            {
                if (ammoReserve > 0 && ammo != ammoCanHold)
                {
                    reloadAudio.Play();
                    isReloading = true;
                    yield return new WaitForSeconds(reloadTime);
                    int ammoNeeded = ammoCanHold - ammo;

                    if (ammoNeeded <= ammoReserve)
                    {
                        ammo += ammoNeeded;
                        ammoReserve -= ammoNeeded;
                    }
                    else if (ammoReserve == 0)
                    { }
                    else
                    {
                        ammo += ammoReserve;
                        ammoReserve = 0;
                    }
                }
                HandleUI();
                // Play reloading animation here
                isReloading = false;
            }
        }
    }

    [PunRPC]
    public void Shoot()
    {
        if (!isReloading)
        {
            shootAudio.Play();
            ammo -= 1;
            HandleUI();
            // Play shooting animation here
            CurrentRecoil1 += new Vector3(RecoilRotation.x, UnityEngine.Random.Range(-RecoilRotation.y, RecoilRotation.y), UnityEngine.Random.Range(-RecoilRotation.z, RecoilRotation.z));
            CurrentRecoil3 += new Vector3(UnityEngine.Random.Range(-RecoilKickBack.x, RecoilKickBack.x), UnityEngine.Random.Range(-RecoilKickBack.y, RecoilKickBack.y), RecoilKickBack.z);

            if (isAimed)
            {
                camCurrentRotation += new Vector3(-camRecoilRotationAiming.x, UnityEngine.Random.Range(-camRecoilRotationAiming.y, camRecoilRotationAiming.y), UnityEngine.Random.Range(-camRecoilRotationAiming.z, camRecoilRotationAiming.z));
            }
            else
            {
                camCurrentRotation += new Vector3(-camRecoilRotation.x, UnityEngine.Random.Range(-camRecoilRotation.y, camRecoilRotation.y), UnityEngine.Random.Range(-camRecoilRotation.z, camRecoilRotation.z));
            }

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



                    //if we hit enemy player
                    if (hit.collider.gameObject.layer == 11)
                    {
                        bool applyDamage = false;

                        if (GameSettings.GameMode == GameMode.FFA)
                        {
                            applyDamage = true;
                        }

                        if (GameSettings.GameMode == GameMode.TDM)
                        {
                            if (hit.collider.transform.root.gameObject.GetComponent<Health>().awayTeam != GameSettings.IsAwayTeam)
                            {
                                applyDamage = true;
                            }
                        }
                        if (applyDamage)
                        {
                            if (hit.collider.gameObject.name == "body") hit.collider.transform.root.gameObject.GetPhotonView().RPC("TakeDamageRPC", RpcTarget.All, damage, PhotonNetwork.LocalPlayer.ActorNumber);
                            if (hit.collider.gameObject.name == "head") hit.collider.transform.root.gameObject.GetPhotonView().RPC("TakeDamageRPC", RpcTarget.All, damage * 2, PhotonNetwork.LocalPlayer.ActorNumber);
                            if (hit.collider.gameObject.name == "left.thigh" || hit.collider.gameObject.name == "right.thigh") hit.collider.transform.root.gameObject.GetPhotonView().RPC("TakeDamageRPC", RpcTarget.All, damage / 2, PhotonNetwork.LocalPlayer.ActorNumber);
                            if (hit.collider.gameObject.name == "right.shoulder" || hit.collider.gameObject.name == "left.shoulder") hit.collider.transform.root.gameObject.GetPhotonView().RPC("TakeDamageRPC", RpcTarget.All, damage / 2, PhotonNetwork.LocalPlayer.ActorNumber);
                        }
                    }
                }
            }
        }
    }

    public RaycastHit GetHitData()// Returns the RayCast Hit so that other scripts will know what was hit
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range, bulletGoThrough))
        {
            return hit;
        }

        return hit;
    }

    public void DropRotation()
    {
        gameObject.transform.Rotate(0, 1, 0);
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
