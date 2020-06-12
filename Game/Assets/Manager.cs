using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using UnityEngine.Video;
using System.IO.IsolatedStorage;
using Photon.Pun.UtilityScripts;
using System.Linq;

namespace FPSGame
{
    public class PlayerInfo
    {
        public ProfileData profile;
        public int actor;
        public short kills;
        public short deaths;
        public bool isDead;
        public bool awayTeam;

        public PlayerInfo(ProfileData p, int a, short k, short d, bool i, bool t)
        {
            this.profile = p;
            this.actor = a;
            this.kills = k;
            this.deaths = d;
            this.isDead = i;
            this.awayTeam = t;
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
        public int matchLength = 180;
        private int currentMatchTime;
        private Coroutine timerCoroutine;

        public string player_prefab;
        public Transform[] spawnpoints;
        public Transform[] awaySpawn;
        public Transform[] homeSpawn;

        public List<PlayerInfo> playerInfo = new List<PlayerInfo>();
        public int myind;

        private bool playerAdded;

        private Text ui_mykills;
        private Text ui_mydeaths;
        private Transform ui_leaderboard;
        private Transform ui_endgame;
        private Text ui_respawntimer;
        private GameObject ui_health;
        private Text ui_timer;
        private Transform ui_winner;
        private Transform ui_killList;
        private Transform ui_team;


        public int mainmenu = 0;
        public int killcount = 3;

        public int winScore = 3;
        public int homeScore = 0;
        public int awayScore = 0;

        public GameObject mapcam;
        private GameState state = GameState.Playing;

        public GameObject StandartWeapon;
        private int lastId;

        public bool perpetual = false;

        private bool respawned = false;

        public enum EventCodes : byte
        {
            NewPlayer,
            UpdatePlayers,
            ChangeStat,
            NewMatch,
            RefreshTimer
        }

        private void Update()
        {
            if (state == GameState.Ending) return;

            if (Input.GetKeyDown(KeyCode.Tab)) Leaderboard(ui_leaderboard);
            if (Input.GetKeyUp(KeyCode.Tab)) ui_leaderboard.gameObject.SetActive(false);
            if (Input.GetKeyDown(KeyCode.M))
            {
                ui_team.gameObject.SetActive(!ui_team.gameObject.activeSelf);
                Pause.paused = ui_team.gameObject.activeSelf;
                Cursor.lockState = (ui_team.gameObject.activeSelf) ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = ui_team.gameObject.activeSelf;
            }
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
            InitializeTimer();
            NewPlayer_S(Launcher.myProfile);
            if (PhotonNetwork.IsMasterClient)
            {
                playerAdded = true;
                Spawn();
            }
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();
            SceneManager.LoadScene(mainmenu);
        }

        public void Spawn()
        {
            Transform t_spawn = spawnpoints[Random.Range(0, spawnpoints.Length)];
            if (GameSettings.GameMode == GameMode.ORIGINAL)
            {
                if (GameSettings.IsAwayTeam)
                {
                    t_spawn = awaySpawn[Random.Range(0, awaySpawn.Length)];
                }
                else
                {
                    t_spawn = homeSpawn[Random.Range(0, homeSpawn.Length)];
                }
            }
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
            ui_timer = GameObject.Find("HUD/Timer/Text").GetComponent<Text>();
            ui_winner = GameObject.Find("HUD").transform.Find("Winner").transform;
            ui_killList = GameObject.Find("HUD/KillList").transform;
            ui_team = GameObject.Find("HUD/Team/Window").transform;

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
            if (GameSettings.GameMode == GameMode.FFA) p_lb = p_lb.Find("FFA");
            if (GameSettings.GameMode == GameMode.ORIGINAL) p_lb = p_lb.Find("ORIGINAL");

            // clean up
            for (int i = 2; i < p_lb.childCount; i++)
            {
                Destroy(p_lb.GetChild(i).gameObject);
            }

            // set details
            p_lb.Find("Header/Mode").GetComponent<Text>().text = System.Enum.GetName(typeof(GameMode), GameSettings.GameMode);
            p_lb.Find("Header/Map").GetComponent<Text>().text = SceneManager.GetActiveScene().name;

            if (GameSettings.GameMode == GameMode.ORIGINAL)
            {
                p_lb.Find("Header/Score/Home").GetComponent<Text>().text = homeScore.ToString();
                p_lb.Find("Header/Score/Away").GetComponent<Text>().text = awayScore.ToString();
            }


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

                if (GameSettings.GameMode == GameMode.ORIGINAL)
                {
                    newcard.transform.Find("Home").gameObject.SetActive(!a.awayTeam);
                    newcard.transform.Find("Away").gameObject.SetActive(a.awayTeam);
                }

                if (t_alternateColors) newcard.GetComponent<Image>().color = new Color32(0, 0, 0, 180);
                t_alternateColors = !t_alternateColors;

                newcard.transform.Find("Username").GetComponent<Text>().text = a.profile.username;
                newcard.transform.Find("Kills Value").GetComponent<Text>().text = a.kills.ToString();
                newcard.transform.Find("Deaths Value").GetComponent<Text>().text = a.deaths.ToString();

                newcard.SetActive(true);
            }

                p_lb.gameObject.SetActive(true);
                p_lb.parent.gameObject.SetActive(true);
        }

        private List<PlayerInfo> SortPlayers(List<PlayerInfo> p_info)
        {
            List<PlayerInfo> sorted = new List<PlayerInfo>();
            if (GameSettings.GameMode == GameMode.FFA)
            {
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
            }

            if (GameSettings.GameMode == GameMode.ORIGINAL)
            {
                List<PlayerInfo> homeSorted = new List<PlayerInfo>();
                List<PlayerInfo> awaySorted = new List<PlayerInfo>();

                int homeSize = 0;
                int awaySize = 0;

                foreach (PlayerInfo p in p_info)
                {
                    if (p.awayTeam) awaySize++;
                    else homeSize++;
                }

                while (homeSorted.Count < homeSize)
                {
                    // set defaults
                    short highest = -1;
                    PlayerInfo selection = p_info[0];

                    // grab next highest player
                    foreach (PlayerInfo a in p_info)
                    {
                        if (a.awayTeam) continue;
                        if (homeSorted.Contains(a)) continue;
                        if (a.kills > highest)
                        {
                            selection = a;
                            highest = a.kills;
                        }
                    }

                    // add player
                    homeSorted.Add(selection);
                }

                while (awaySorted.Count < awaySize)
                {
                    // set defaults
                    short highest = -1;
                    PlayerInfo selection = p_info[0];

                    // grab next highest player
                    foreach (PlayerInfo a in p_info)
                    {
                        if (!a.awayTeam) continue;
                        if (awaySorted.Contains(a)) continue;
                        if (a.kills > highest)
                        {
                            selection = a;
                            highest = a.kills;
                        }
                    }

                    // add player
                    awaySorted.Add(selection);
                }

                sorted.AddRange(homeSorted);
                sorted.AddRange(awaySorted);
            }
            return sorted;
        }

        public List<PlayerInfo> SortHome(List<PlayerInfo> p_info)
        {

            List<PlayerInfo> sorted = new List<PlayerInfo>();
            List<PlayerInfo> homeSorted = new List<PlayerInfo>();

            int homeSize = 0;

            foreach (PlayerInfo p in p_info)
            {
                if (!p.awayTeam) homeSize++;
            }

            while (homeSorted.Count < homeSize)
            {
                // set defaults
                short highest = -1;
                PlayerInfo selection = p_info[0];

                // grab next highest player
                foreach (PlayerInfo a in p_info)
                {
                    if (a.awayTeam) continue;
                    if (homeSorted.Contains(a)) continue;
                    if (a.kills > highest)
                    {
                        selection = a;
                        highest = a.kills;
                    }
                }

                // add player
                homeSorted.Add(selection);
            }

            return homeSorted;
        }

        public List<PlayerInfo> SortAway(List<PlayerInfo> p_info)
        {

            List<PlayerInfo> sorted = new List<PlayerInfo>();
            List<PlayerInfo> awaySorted = new List<PlayerInfo>();

            int awaySize = 0;

            foreach (PlayerInfo p in p_info)
            {
                if (p.awayTeam) awaySize++;
            }

            while (awaySorted.Count < awaySize)
            {
                // set defaults
                short highest = -1;
                PlayerInfo selection = p_info[0];

                // grab next highest player
                foreach (PlayerInfo a in p_info)
                {
                    if (!a.awayTeam) continue;
                    if (awaySorted.Contains(a)) continue;
                    if (a.kills > highest)
                    {
                        selection = a;
                        highest = a.kills;
                    }
                }

                // add player
                awaySorted.Add(selection);
            }

            return awaySorted;
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
            if (GameSettings.GameMode == GameMode.FFA)
            {
                foreach (PlayerInfo a in playerInfo)
                {
                    if (a.kills >= killcount)
                    {
                        detectwin = true;
                        break;
                    }
                }
            }
            if(GameSettings.GameMode == GameMode.ORIGINAL)
            {
                if(awayScore >= winScore)
                {
                    ui_winner.Find("Red").gameObject.SetActive(true);
                    detectwin = true;
                }
                if (homeScore >= winScore)
                {
                    ui_winner.Find("Blue").gameObject.SetActive(true);
                    detectwin = true;
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

            if (timerCoroutine != null) StopCoroutine(timerCoroutine);
            currentMatchTime = 0;
            RefreshTimerUI();

            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.DestroyAll();

                if (!perpetual)
                {
                    PhotonNetwork.CurrentRoom.IsVisible = false;
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                }
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
                case EventCodes.NewMatch:
                    NewMatch_R();
                    break;
                case EventCodes.RefreshTimer:
                    RefreshTimer_R(o);
                    break;
            }
        }

        private bool CalculateTeam()
        {
            if(GameSettings.GameMode == GameMode.FFA) return false;
            if (GameSettings.GameMode == GameMode.ORIGINAL) return PhotonNetwork.CurrentRoom.PlayerCount % 2 == 0;
            else return false;
        }

        public void NewPlayer_S(ProfileData p)
        {
            object[] package = new object[6];

            package[0] = p.username;
            package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
            package[2] = (short)0;
            package[3] = (short)0;
            package[4] = (bool)false;
            package[5] = CalculateTeam();

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
                (short)data[3],
                (bool)data[4],
                (bool)data[5]
            );

            playerInfo.Add(p);

            foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("Player"))
            {
                gameObject.GetComponent<Health>().TrySync();
            }

            UpdatePlayers_S((int)state, playerInfo);
        }

        public void UpdatePlayers_S(int state, List<PlayerInfo> info)
        {
            object[] package = new object[info.Count + 1];

            package[0] = state;
            for (int i = 0; i < info.Count; i++)
            {
                object[] piece = new object[6];

                piece[0] = info[i].profile.username;
                piece[1] = info[i].actor;
                piece[2] = info[i].kills;
                piece[3] = info[i].deaths;
                piece[4] = info[i].isDead;
                piece[5] = info[i].awayTeam;

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

            if(playerInfo.Count < data.Length - 1)
            {
                foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("Player"))
                {
                    gameObject.GetComponent<Health>().TrySync();
                }
            }

            playerInfo = new List<PlayerInfo>();

            for (int i = 1; i < data.Length; i++)
            {
                object[] extract = (object[])data[i];

                PlayerInfo p = new PlayerInfo(
                    new ProfileData(
                        (string)extract[0]),
                    (int)extract[1],
                    (short)extract[2],
                    (short)extract[3],
                    (bool)extract[4],
                    (bool)extract[5]
                    );
                playerInfo.Add(p);

                if (PhotonNetwork.LocalPlayer.ActorNumber == p.actor)
                {
                    myind = i - 1;

                    if (!playerAdded)
                    {
                        playerAdded = true;
                        GameSettings.IsAwayTeam = p.awayTeam;
                        Spawn();
                    }
                }
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
                            playerInfo[i].isDead = true;
                            Debug.LogError($"Player {playerInfo[i].profile.username} : deaths = {playerInfo[i].deaths}");
                            break;
                        case 2:
                            playerInfo[i].isDead = false;
                            Debug.LogError($"Player {playerInfo[i].profile.username} respawned");
                            break;
                        case 3:
                            playerInfo[i].awayTeam = true;
                            foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("Player"))
                            {
                                gameObject.GetComponent<Health>().TrySync();
                            }
                            break;

                        case 4:
                            playerInfo[i].awayTeam = false;
                            foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("Player"))
                            {
                                gameObject.GetComponent<Health>().TrySync();
                            }
                            break;
                    }

                    if (i == myind) RefreshMyStats();
                    if (ui_leaderboard.gameObject.activeSelf) Leaderboard(ui_leaderboard);

                    break;
                }
            }
            //Если убивает не мастерклиент то создается копия этого не мастерклиента. Потому что когда умирает мастерклиент то он только отправляет статы, но не получает ничего.
            if (GameSettings.GameMode == GameMode.ORIGINAL)
            {
                if (stat == 1 || stat == 0)
                {
                    List<PlayerInfo> list = SortAway(playerInfo);
                    //до этой проверки нужно установить игроку isDead true
                    if (list.All(x => x.isDead) && !respawned)
                    {
                        Debug.LogWarning("away respawned");
                        respawned = true;
                        StartCoroutine(NewRound());
                        awayScore += 1;
                        ui_winner.Find("Red").gameObject.SetActive(true);
                    }
                    list = SortHome(playerInfo);
                    if (list.All(x => x.isDead) && !respawned)
                    {
                        Debug.LogWarning("home respawned");
                        respawned = true;
                        StartCoroutine(NewRound());
                        homeScore += 1;
                        ui_winner.Find("Blue").gameObject.SetActive(true);
                    }
                }
            }

            ScoreCheck();
        }
        
        private IEnumerator NewRound()
        {
            yield return new WaitForSeconds(7f);
            DestroyWeapons();
            DestroyPlayers();
            Debug.LogWarning("Respawning");
            ui_winner.Find("Blue").gameObject.SetActive(false);
            ui_winner.Find("Red").gameObject.SetActive(false);
            StartCoroutine("RespawnTimer");
        }

        public void DestroyPlayers()
        {
            foreach (GameObject Player in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (Player.GetComponent<PhotonView>().IsMine == true && PhotonNetwork.IsConnected == true)
                {
                    PhotonNetwork.Destroy(Player.gameObject);
                }
            }
        }
        public void DestroyWeapons()
        {
            foreach (GameObject Weapon in GameObject.FindGameObjectsWithTag("Gun"))
            {
                {
                    PhotonNetwork.Destroy(Weapon.gameObject);
                }
            }
        }

        public void NewMatch_S()
        {
            PhotonNetwork.RaiseEvent(
                (byte)EventCodes.NewMatch,
                null,
                new RaiseEventOptions { Receivers = ReceiverGroup.All },
                new SendOptions { Reliability = true }
                );
        }

        public void NewMatch_R()
        {
            state = GameState.Waiting;
            mapcam.SetActive(false);
            ui_endgame.gameObject.SetActive(false);

            foreach(PlayerInfo p in playerInfo)
            {
                p.kills = 0;
                p.deaths = 0;
            }

            RefreshMyStats();
            InitializeTimer();
            Spawn();
        }

        public void RefreshTimer_S()
        {
            object[] package = new object[] { currentMatchTime };

            PhotonNetwork.RaiseEvent(
                (byte)EventCodes.RefreshTimer,
                package,
                new RaiseEventOptions { Receivers = ReceiverGroup.All },
                new SendOptions { Reliability = true }
            );
        }
        public void RefreshTimer_R(object[] data)
        {
            currentMatchTime = (int)data[0];
            RefreshTimerUI();
        }


        private IEnumerator End(float sec)
        {
            yield return new WaitForSeconds(sec);

            if (perpetual)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    NewMatch_S();
                }
            }
            else
            {
                PhotonNetwork.AutomaticallySyncScene = false;
                PhotonNetwork.LeaveRoom();
            }
        }
        
        private IEnumerator RespawnTimer()
        {
            ui_health = GameObject.Find("Canvas/HUD/Health");
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
            ChangeStat_S(PhotonNetwork.LocalPlayer.ActorNumber, 2, 0);
            respawned = false;
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

        private void RefreshTimerUI()
        {
            string minutes = (currentMatchTime / 60).ToString("00");
            string seconds = (currentMatchTime % 60).ToString("00");
            ui_timer.text = $"{minutes}:{seconds}";
        }
        private void InitializeTimer()
        {
            currentMatchTime = matchLength;
            RefreshTimerUI();

            if (PhotonNetwork.IsMasterClient)
            {
                timerCoroutine = StartCoroutine(Timer());
            }
        }

        private IEnumerator Timer()
        {
            yield return new WaitForSeconds(1f);

            currentMatchTime -= 1;

            if (currentMatchTime <= 0)
            {
                timerCoroutine = null;
                UpdatePlayers_S((int)GameState.Ending, playerInfo);
            }
            else
            {
                RefreshTimer_S();
                timerCoroutine = StartCoroutine(Timer());
            }
        }

        public void KillAdd(int player1_id, int player2_id)
        {
            GameObject kill = GameObject.Find("HUD/KillList/Kill");
            GameObject newkill = Instantiate(kill, ui_killList) as GameObject;
            newkill.gameObject.SetActive(true);
            newkill.transform.Find("Player1").GetComponent<Text>().text = playerInfo[player1_id - 1].profile.username;
            if(playerInfo[player1_id - 1].awayTeam)
            {
                newkill.transform.Find("Player1").GetComponent<Text>().color = new Color(0, 3, 255, 255);
            }
            else newkill.transform.Find("Player1").GetComponent<Text>().color = Color.red;

            newkill.transform.Find("Player2").GetComponent<Text>().text = playerInfo[player2_id - 1].profile.username;
            if (playerInfo[player2_id - 1].awayTeam)
            {
                newkill.transform.Find("Player2").GetComponent<Text>().color = new Color(0, 3, 255, 255);
            }
            else newkill.transform.Find("Player2").GetComponent<Text>().color = Color.red;
            Destroy(newkill, 7f);
        }
        
        
        public void PickAway()
        {
            ChangeStat_S(PhotonNetwork.LocalPlayer.ActorNumber, 3, 0);
            GameSettings.IsAwayTeam = true;
            ui_team.gameObject.SetActive(false);
            Pause.paused = false;
            //StartCoroutine(RespawnTimer());
        }
        public void PickHome()
        {
            ChangeStat_S(PhotonNetwork.LocalPlayer.ActorNumber, 4, 0);
            GameSettings.IsAwayTeam = false;
            ui_team.gameObject.SetActive(false);
            Pause.paused = false;
            //StartCoroutine(RespawnTimer());
        }
    }
}