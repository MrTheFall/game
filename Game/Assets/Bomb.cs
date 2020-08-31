using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Bomb : MonoBehaviourPunCallbacks
{
    private bool onBombPlant = false;
    private Transform ui_bombbar;
    private float planting_status = 0f;
    public bool isPlanted = false;
    public Transform groundCheck;

    public string bomb_prefab;
    
    // Start is called before the first frame update
    void Start()
    {
        ui_bombbar = GameObject.Find("Canvas/HUD/Bomb/Bar").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            if (Input.GetKey(KeyCode.E))
            {
                if (onBombPlant && !isPlanted)
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
            if (planting_status >= 100f && !isPlanted)
            {
                isPlanted = true;
                photonView.RPC("ChangePlantStatus", RpcTarget.AllBufferedViaServer, true);
                SpawnBomb();
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
        isPlanted = status;

    }

    public void SpawnBomb()
    {
        PhotonNetwork.Instantiate(bomb_prefab, groundCheck.position, new Quaternion(0, groundCheck.rotation.y, 0, 0));
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
