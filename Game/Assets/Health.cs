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
    // Start is called before the first frame update
    void Start()
    {
        current_health = max_health;
        manager = GameObject.Find("Manager").GetComponent<Manager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U)) TakeDamage(50);
    }

    [PunRPC]
    public void TakeDamageRPC(int damage)
    {
        TakeDamage(damage);
    }
    public void TakeDamage(int damage)
    {
        if (photonView.IsMine)
        {
            current_health -= damage;
            Debug.LogError(current_health);
            if (current_health <= 0)
            {
                manager.Spawn();
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}
