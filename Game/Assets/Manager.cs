﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using UnityEngine.Video;
using System.IO.IsolatedStorage;

namespace FPSGame
{
    public class PlayerInfo
    {
        public ProfileData profile;
        public int actor;
        public short kills;
        public short deaths;

        public PlayerInfo(ProfileData p, int a, short k, short d)
        {
            this.profile = p;
            this.actor = a;
            this.kills = k;
            this.deaths = d;
        }
    }

    public enum GameState
    {
        Waiting = 0,
        Starting = 1,
        Playing = 2,
        Ending = 3
    }

    public class Manager : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        public string player_prefab;
        public Transform[] spawnpoints;

        public List<PlayerInfo> playerInfo = new List<PlayerInfo>();
        public int myind;

        private Text ui_mykills;
        private Text ui_mydeaths;
        private Transform ui_leaderboard;
        private Transform ui_endgame;
        private Text ui_respawntimer;
        private GameObject ui_health;

        public int mainmenu = 0;
        public int killcount = 3;

        public GameObject mapcam;
        private GameState state = GameState.Playing;

        public GameObject StandartWeapon;
        private int lastId;

        public enum EventCodes : byte
        {
            NewPlayer,
            UpdatePlayers,
            ChangeStat
        }

        private void Update()
        {
            if (state == GameState.Ending) return;

            if (Input.GetKeyDown(KeyCode.Tab)) Leaderboard(ui_leaderboard);
            if (Input.GetKeyUp(KeyCode.Tab)) ui_leaderboard.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        private void Start()
        {
            lastId = 0;

            StandartWeapon = (GameObject)Resources.Load("Weapon/Deagle/Deagle");
            mapcam.SetActive(false);

            ValidateConnection();
            InitializeUI();
            NewPlayer_S(Launcher.myProfile);
            Spawn();
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();
            SceneManager.LoadScene(mainmenu);
        }

        public void Spawn()
        {
            Transform t_spawn = spawnpoints[Random.Range(0, spawnpoints.Length)];
            PhotonNetwork.Instantiate(player_prefab, t_spawn.position, t_spawn.rotation);
        }
        
        public void WaitBeforeSpawn()
        {

        }

        private void InitializeUI()
        {
            ui_mykills = GameObject.Find("HUD/Stats/Kills").GetComponent<Text>();
            ui_mydeaths = GameObject.Find("HUD/Stats/Deaths").GetComponent<Text>();
            ui_leaderboard = GameObject.Find("HUD").transform.Find("Leaderboard").transform;
            ui_endgame = GameObject.Find("HUD").transform.Find("End Game").transform;

            RefreshMyStats();
        }

        private void RefreshMyStats()
        {
            if (playerInfo.Count > myind)
            {
                ui_mykills.text = $"K: {playerInfo[myind].kills}";
                ui_mydeaths.text = $"D: {playerInfo[myind].deaths}";

            }
            else
            {
                ui_mykills.text = "K: 0";
                ui_mydeaths.text = "D: 0";
            }
        }

        private void Leaderboard(Transform p_lb)
        {
            // clean up
            for (int i = 2; i < p_lb.childCount; i++)
            {
                Destroy(p_lb.GetChild(i).gameObject);
            }

            // set details

            p_lb.Find("Header/Map").GetComponent<Text>().text = "NULL";


            // cache prefab
            GameObject playercard = p_lb.GetChild(1).gameObject;
            playercard.SetActive(false);

            // sort
            List<PlayerInfo> sorted = SortPlayers(playerInfo);

            // display
            bool t_alternateColors = false;
            foreach (PlayerInfo a in sorted)
            {
                GameObject newcard = Instantiate(playercard, p_lb) as GameObject;

                if (t_alternateColors) newcard.GetComponent<Image>().color = new Color32(0, 0, 0, 180);
                t_alternateColors = !t_alternateColors;

                newcard.transform.Find("Username").GetComponent<Text>().text = a.profile.username;
                newcard.transform.Find("Kills Value").GetComponent<Text>().text = a.kills.ToString();
                newcard.transform.Find("Deaths Value").GetComponent<Text>().text = a.deaths.ToString();

                newcard.SetActive(true);
            }

            // activate
            p_lb.gameObject.SetActive(true);
            p_lb.parent.gameObject.SetActive(true);
        }

        private List<PlayerInfo> SortPlayers(List<PlayerInfo> p_info)
        {
            List<PlayerInfo> sorted = new List<PlayerInfo>();
            while (sorted.Count < p_info.Count)
            {
                // set defaults
                short highest = -1;
                PlayerInfo selection = p_info[0];

                // grab next highest player
                foreach (PlayerInfo a in p_info)
                {
                    if (sorted.Contains(a)) continue;
                    if (a.kills > highest)
                    {
                        selection = a;
                        highest = a.kills;
                    }
                }

                // add player
                sorted.Add(selection);
            }
            return sorted;
        }

        private void ValidateConnection()
        {
            if (PhotonNetwork.IsConnected) return;
            SceneManager.LoadScene(mainmenu);
        }

        private void StateCheck()
        {
            if (state == GameState.Ending) EndGame();
        }

        private void ScoreCheck()
        {
            bool detectwin = false;

            foreach (PlayerInfo a in playerInfo)
            {
                if (a.kills >= killcount)
                {
                    detectwin = true;
                    break;
                }
            }
            if (detectwin)
            {
                if (PhotonNetwork.IsMasterClient && state != GameState.Ending)
                {
                    UpdatePlayers_S((int)GameState.Ending, playerInfo);
                }
            }
        }

        private void EndGame()
        {
            state = GameState.Ending;

            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.DestroyAll();
                PhotonNetwork.CurrentRoom.IsVisible = false;
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }

            mapcam.SetActive(true);
            ui_endgame.gameObject.SetActive(true);
            Leaderboard(ui_endgame.Find("Leaderboard"));

            StartCoroutine(End(6f));
        }

        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code >= 200) return;

            EventCodes e = (EventCodes)photonEvent.Code;
            object[] o = (object[])photonEvent.CustomData;

            switch (e)
            {
                case EventCodes.NewPlayer:
                    NewPlayer_R(o);
                    break;
                case EventCodes.UpdatePlayers:
                    UpdatePlayers_R(o);
                    break;
                case EventCodes.ChangeStat:
                    ChangeStat_R(o);
                    break;
            }
        }
        public void NewPlayer_S(ProfileData p)
        {
            object[] package = new object[7];

            package[0] = p.username;
            package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
            package[2] = (short)0;
            package[3] = (short)0;

            PhotonNetwork.RaiseEvent(
                (byte)EventCodes.NewPlayer,
                package,
                new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
                new SendOptions { Reliability = true }
            );
        }

        public void NewPlayer_R(object[] data)
        {
            PlayerInfo p = new PlayerInfo(
                new ProfileData((string)data[0]),
                (int)data[1],
                (short)data[2],
                (short)data[3]
            );

            playerInfo.Add(p);

            UpdatePlayers_S((int)state, playerInfo);
        }

        public void UpdatePlayers_S(int state, List<PlayerInfo> info)
        {
            object[] package = new object[info.Count + 1];

            package[0] = state;
            for (int i = 0; i < info.Count; i++)
            {
                object[] piece = new object[4];

                piece[0] = info[i].profile.username;
                piece[1] = info[i].actor;
                piece[2] = info[i].kills;
                piece[3] = info[i].deaths;

                package[i + 1] = piece;
            }

            PhotonNetwork.RaiseEvent(
                (byte)EventCodes.UpdatePlayers,
                package,
                new RaiseEventOptions { Receivers = ReceiverGroup.All },
                new SendOptions { Reliability = true }
            );
        }
        public void UpdatePlayers_R(object[] data)
        {
            state = (GameState)data[0];
            playerInfo = new List<PlayerInfo>();

            for (int i = 1; i < data.Length; i++)
            {
                object[] extract = (object[])data[i];

                PlayerInfo p = new PlayerInfo(
                    new ProfileData(
                        (string)extract[0]),
                    (int)extract[1],
                    (short)extract[2],
                    (short)extract[3]
                    );
                playerInfo.Add(p);
                if (PhotonNetwork.LocalPlayer.ActorNumber == p.actor) myind = i - 1;
            }
            StateCheck();
        }
        public void ChangeStat_S(int actor, byte stat, byte amt)
        {
            object[] package = new object[] { actor, stat, amt };

            PhotonNetwork.RaiseEvent(
                (byte)EventCodes.ChangeStat,
                package,
                new RaiseEventOptions { Receivers = ReceiverGroup.All },
                new SendOptions { Reliability = true }
            );
        }

        public void ChangeStat_R(object[] data)
        {
            int actor = (int)data[0];
            byte stat = (byte)data[1];
            byte amt = (byte)data[2];

            for (int i = 0; i < playerInfo.Count; i++)
            {
                if (playerInfo[i].actor == actor)
                {
                    switch (stat)
                    {
                        case 0:
                            playerInfo[i].kills += amt;
                            Debug.LogError($"Player {playerInfo[i].profile.username} : kills = {playerInfo[i].kills}");
                            break;

                        case 1:
                            playerInfo[i].deaths += amt;
                            Debug.LogError($"Player {playerInfo[i].profile.username} : deaths = {playerInfo[i].deaths}");
                            break;
                    }

                    if (i == myind) RefreshMyStats();
                    if (ui_leaderboard.gameObject.activeSelf) Leaderboard(ui_leaderboard);

                    break;
                }
            }

            ScoreCheck();
        }

        private IEnumerator End(float sec)
        {
            yield return new WaitForSeconds(sec);

            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.LeaveRoom();
        }

        private IEnumerator RespawnTimer()
        {
            ui_health = GameObject.Find("HUD/Health");
            ui_health.SetActive(false);
            mapcam.SetActive(true);
            ui_respawntimer = GameObject.Find("HUD").transform.Find("RespawnTimer").Find("Timer").GetComponent<Text>();
            ui_respawntimer.gameObject.SetActive(true);
            for (int i = 5; i >= 1; i--)
            {
                ui_respawntimer.text = "Respawn in: " + i.ToString() + " sec";
                yield return new WaitForSeconds(1f);
            }
            Spawn();
            ui_respawntimer.gameObject.SetActive(false);
            ui_respawntimer.text = "Respawn in: 5 sec";
            mapcam.SetActive(false);
            ui_health.SetActive(true);
        }

        public void HilightWeapon(int lastId, int id)
        {
            GameObject.Find("Canvas/Pause/Pause/PauseMenu/ChooseWeapon/Scroll View/Viewport/Content/Container").transform.GetChild(lastId).GetComponent<Image>().color = new Color32(60, 60, 60, 150);
            GameObject.Find("Canvas/Pause/Pause/PauseMenu/ChooseWeapon/Scroll View/Viewport/Content/Container").transform.GetChild(id).GetComponent<Image>().color = new Color32(0, 0, 0, 150);
        }

        public void ChooseWeapon(int id)
        {
            switch (id)
            {
                case 1: 
                    StandartWeapon = (GameObject)Resources.Load("Weapon/Deagle/Deagle");
                    break;
                case 2: 
                    StandartWeapon = (GameObject)Resources.Load("Weapon/M4A1/M4A1");
                    break;
            }
            HilightWeapon(lastId, id - 1);
            lastId = id - 1;
        }
    }
}