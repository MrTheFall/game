﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using FPSGame;
using UnityEngine.UI;
using System.Linq;
using System;

public class Health : MonoBehaviourPunCallbacks
{

    public int max_health = 100;
    private int current_health;
    private Manager manager;
    private Transform ui_healthbar;

    public bool awayTeam;
    public Renderer[] teamIndicators;
    private Text ui_team;


    // Start is called before the first frame update
    void Start()
    {
        current_health = max_health;
        manager = GameObject.Find("Manager").GetComponent<Manager>();
        if (photonView.IsMine)
        {
            awayTeam = GameSettings.IsAwayTeam;
            ui_healthbar = GameObject.Find("Canvas/HUD/Health/Bar").transform;
            refreshHealthBar();
            ui_team = GameObject.Find("HUD/Stats/Team/Text").GetComponent<Text>();


            if (GameSettings.GameMode == GameMode.ORIGINAL)
            {
                photonView.RPC("SyncTeam", RpcTarget.All, GameSettings.IsAwayTeam);
                if (awayTeam)
                {
                    ui_team.text = "BLUE Team";
                    ui_team.color = Color.blue;
                }
                else
                {
                    ui_team.text = "RED Team";
                    ui_team.color = Color.red;
                }
            }
            else
            {
                ui_team.gameObject.SetActive(false);
            }

        }
    }

    [PunRPC]
    private void SyncTeam(bool p_awayTeam)
    {
        if (photonView.IsMine)
        {
            if (awayTeam != p_awayTeam)
            {
                awayTeam = p_awayTeam;
                Debug.LogWarning("Team Changed");
                photonView.RPC("TakeDamageRPC", RpcTarget.All, 999, -1);
            }
        }
        awayTeam = p_awayTeam;
        
        var arrayOfChildren = gameObject.transform.Cast<Transform>().Where(c => c.gameObject.tag == "PlayerModel").ToArray();
        if (awayTeam)
        {
            // ColorTeamIndicators(Color.blue);
            foreach (var obj in arrayOfChildren)
            {
                obj.transform.Find("Red").gameObject.SetActive(false);
                obj.transform.Find("Blue").gameObject.SetActive(true);

            }
        }
        else
        {
            // ColorTeamIndicators(Color.red);
            foreach (var obj in arrayOfChildren)
            {
                obj.transform.Find("Blue").gameObject.SetActive(false);
                obj.transform.Find("Red").gameObject.SetActive(true);
            }
        }
    }

    /* private void ColorTeamIndicators(Color p_color)
    {
        // foreach (Renderer renderer in teamIndicators) renderer.material.color = p_color;
    } */

    public void TrySync()
    {
        if (!photonView.IsMine) return;
        if(GameSettings.GameMode == GameMode.ORIGINAL)
        {
            photonView.RPC("SyncTeam", RpcTarget.All, GameSettings.IsAwayTeam);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            refreshHealthBar();
        }
        if (Input.GetKeyDown(KeyCode.U)) TakeDamage(50, -1);
    }

    [PunRPC]
    public void TakeDamageRPC(int damage, int p_actor)
    {
        TakeDamage(damage, p_actor);
    }

    public void refreshHealthBar()
    {   
        float t_health_ratio = (float)current_health / (float)max_health;
        ui_healthbar.localScale = Vector3.Lerp(ui_healthbar.localScale, new Vector3(t_health_ratio, 1, 1), Time.deltaTime * 8f);
    }

    public void TakeDamage(int damage, int p_actor)
    {
        if (photonView.IsMine)
        {
            current_health -= damage;
            Debug.LogError(current_health);
            if (current_health <= 0)
            {
                Debug.LogWarning(PhotonNetwork.LocalPlayer.ActorNumber);
                if (p_actor >= 0) photonView.RPC("KillAddRPC", RpcTarget.All, p_actor, PhotonNetwork.LocalPlayer.ActorNumber);
                photonView.RPC("DropWeapon", RpcTarget.All);
                if (GameSettings.GameMode == GameMode.ORIGINAL)
                {
                    manager.mapcam.SetActive(true);
                    ui_healthbar.parent.gameObject.SetActive(false);
                }
                PhotonNetwork.Destroy(gameObject);
                manager.ChangeStat_S(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);
                if (GameSettings.GameMode == GameMode.FFA) manager.StartCoroutine("RespawnTimer");


                if (p_actor >= 0)
                {
                    manager.ChangeStat_S(p_actor, 0, 1);
                }
            }
        }
    }

    [PunRPC]
    public void KillAddRPC(int player1_id, int player2_id)
    {
        manager.KillAdd(player1_id, player2_id);
    }

}
