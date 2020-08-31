using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using FPSGame;

public class Bomb : MonoBehaviourPunCallbacks
{
    private bool onBombPlant = false;
    private Transform ui_bombbar;
    private Manager manager;
    private float planting_status = 0f;
    public Transform groundCheck;
    public Camera cam;
    public float defuseRange;

    public string bomb_prefab;
    
    // Start is called before the first frame update
    void Start()
    {
        ui_bombbar = GameObject.Find("Canvas/HUD/Bomb/Bar").transform;
        manager = GameObject.Find("Manager").GetComponent<Manager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            if (Input.GetKey(KeyCode.E))
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                Physics.Raycast(ray, out hit, defuseRange);
                if (onBombPlant && !manager.isBombPlanted && !GameSettings.IsAwayTeam)
                {
                    ui_bombbar.parent.gameObject.SetActive(true);
                    planting_status += Time.deltaTime * 25;
                }
                else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Bomb") && manager.isBombPlanted && GameSettings.IsAwayTeam)
                {
                    ui_bombbar.parent.gameObject.SetActive(true);
                    planting_status += Time.deltaTime * 25;
                }
                else
                {
                    ui_bombbar.parent.gameObject.SetActive(false);
                    planting_status = 0;
                }

            }
            if (Input.GetKeyUp(KeyCode.E))
            {
                ui_bombbar.parent.gameObject.SetActive(false);
                planting_status = 0;
            }
            if (planting_status >= 100f && !manager.isBombPlanted && !GameSettings.IsAwayTeam)
            {
                manager.isBombPlanted = true;
                photonView.RPC("ChangePlantStatus", RpcTarget.AllBufferedViaServer, true);
                SpawnBomb();
                ui_bombbar.parent.gameObject.SetActive(false);
                planting_status = 0;
            }
            if (planting_status >= 100f && manager.isBombPlanted && GameSettings.IsAwayTeam)
            {
                manager.isBombPlanted = false;
                photonView.RPC("ChangePlantStatus", RpcTarget.AllBufferedViaServer, false);
                ui_bombbar.parent.gameObject.SetActive(false);
                planting_status = 0;
            }

            refreshBombBar();
        }
    }
    public void refreshBombBar()
    {
        float bomb_ratio = (float)planting_status / (float)100;
        ui_bombbar.localScale = Vector3.Lerp(ui_bombbar.localScale, new Vector3(bomb_ratio, 1, 1), Time.deltaTime * 8f);
    }

    [PunRPC]
    public void ChangePlantStatus(bool status)
    {
        manager.isBombPlanted = status;

    }

    public void SpawnBomb()
    {
        PhotonNetwork.Instantiate(bomb_prefab, groundCheck.position - new Vector3(0, 0.21f, 0), new Quaternion(0, groundCheck.rotation.y, 0, 0));
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "BombPlant")
        {
            onBombPlant = true;
        }
    }
    void OnTriggerExit(Collider collider)
    {
        if (collider.tag == "BombPlant")
        {
            onBombPlant = false;
        }
    }
}
