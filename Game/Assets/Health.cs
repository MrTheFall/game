using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using FPSGame;

public class Health : MonoBehaviourPunCallbacks
{

    public int max_health = 100;
    private int current_health;
    private Manager manager;
    private Transform ui_healthbar;

    // Start is called before the first frame update
    void Start()
    {
        current_health = max_health;
        manager = GameObject.Find("Manager").GetComponent<Manager>();
        if (photonView.IsMine)
        {
            ui_healthbar = GameObject.Find("Canvas/HUD/Health/Bar").transform;
            refreshHealthBar();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            refreshHealthBar();
        }
        if (Input.GetKeyDown(KeyCode.U)) TakeDamage(50);
    }

    [PunRPC]
    public void TakeDamageRPC(int damage)
    {
        TakeDamage(damage);
    }

    public void refreshHealthBar()
    {   
        float t_health_ratio = (float)current_health / (float)max_health;
        ui_healthbar.localScale = Vector3.Lerp(ui_healthbar.localScale, new Vector3(t_health_ratio, 1, 1), Time.deltaTime * 8f);
    }

    public void TakeDamage(int damage)
    {
        if (photonView.IsMine)
        {
            current_health -= damage;
            Debug.LogError(current_health);
            if (current_health <= 0)
            {
                photonView.RPC("DropWeapon", RpcTarget.All);
                manager.Spawn();
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}
