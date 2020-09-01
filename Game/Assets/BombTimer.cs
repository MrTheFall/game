using FPSGame;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombTimer : MonoBehaviourPunCallbacks
{
    public float bombTimer;
    public bool isExploded = false;
    private Manager manager;
    

    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.Find("Manager").GetComponent<Manager>();
    }

    // Update is called once per frame
    void Update()
    {
        bombTimer -= Time.deltaTime;
        if (bombTimer < 0 && !isExploded)
        {
            Explosion();
            isExploded = true;
        }
    }

    private void Explosion()
    {
        Debug.LogWarning("BOMB EXPLODED. BOOM!");
        manager.photonView.RPC("BombExplosionRoundEnd", RpcTarget.All);

    }

    [PunRPC]
    public void Defuse()
    {
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

}
