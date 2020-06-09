using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Security.Cryptography;
using UnityEngine.UI;

namespace FPSGame
{

    [System.Serializable]
    public class ProfileData
    {
        public string username;

        public ProfileData()
        {
            this.username = "";
        }

        public ProfileData(string u)
        {
            this.username = u;
        }
    }

    [System.Serializable]
    public class MapData
    {
        public string name;
        public int scene;
    }

    public class Launcher : MonoBehaviourPunCallbacks
    {
        public InputField usernameField;
        public static ProfileData myProfile = new ProfileData();
        public InputField roomnameField;
        public Text mapValue;
        public Slider maxPlayersSlider;
        public Text maxPlayersValue;

        public GameObject tabMain;
        public GameObject tabRooms;
        public GameObject tabCreate;

        public GameObject buttonRoom;

        private List<RoomInfo> roomList;
        public MapData[] maps;
        private int currentmap = 0;

        public void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            Connect();
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("Joined");

            PhotonNetwork.JoinLobby();
            base.OnConnectedToMaster();
        }

        public override void OnJoinedRoom()
        {
            StartGame();
            base.OnJoinedRoom();
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Create();
            base.OnJoinRandomFailed(returnCode, message);
        }

        public void Connect()
        {
            Debug.Log("Trying to connect...");
            PhotonNetwork.GameVersion = "0.0.0";
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        public void Join()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.JoinRandomRoom();
            Debug.Log("Joined Random Room");
        }

        public void Create()
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = (byte)maxPlayersSlider.value;

            options.CustomRoomPropertiesForLobby = new string[] { "map" };

            // will use this later
            ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
            properties.Add("map", currentmap);
            options.CustomRoomProperties = properties;

            PhotonNetwork.CreateRoom(roomnameField.text, options);
            Debug.Log("Created new room");
        }

        public void ChangeMap()
        {
            currentmap++;
            if (currentmap >= maps.Length) currentmap = 0;
            mapValue.text = "MAP: " + maps[currentmap].name.ToUpper();
        }
        
        public void ChangeMode()
        {

        }

        public void ChangeMaxPlayersSlider(float t_value)
        {
            maxPlayersValue.text = Mathf.RoundToInt(t_value).ToString();
        }

        public void TabCloseAll()
        {
            tabMain.SetActive(false);
            tabRooms.SetActive(false);
            tabCreate.SetActive(false);
        }   

        public void TabOpenRooms()
        {
            TabCloseAll();
            tabRooms.SetActive(true);
        }

        public void TabOpenMain()
        {
            TabCloseAll();
            tabMain.SetActive(true);
        }

        public void TabOpenCreate()
        {
            TabCloseAll();
            tabCreate.SetActive(true);

            roomnameField.text = "";

            currentmap = 0;
            mapValue.text = "MAP: " + maps[currentmap].name.ToUpper();

            maxPlayersSlider.value = maxPlayersSlider.maxValue;
            maxPlayersValue.text = Mathf.RoundToInt(maxPlayersSlider.value).ToString();
        }

            public void ClearRoomList()
        {
            Transform content = tabRooms.transform.Find("Scroll View/Viewport/Content");
            foreach (Transform a in content) Destroy(a.gameObject);
        }

        public override void OnRoomListUpdate(List<RoomInfo> p_list)
        {
            roomList = p_list;
            ClearRoomList();

            Transform content = tabRooms.transform.Find("Scroll View/Viewport/Content");

            foreach (RoomInfo a in roomList)
            {
                GameObject newRoomButton = Instantiate(buttonRoom, content) as GameObject;

                newRoomButton.transform.Find("Name").GetComponent<Text>().text = a.Name;
                newRoomButton.transform.Find("Players").GetComponent<Text>().text = a.PlayerCount + " / " + a.MaxPlayers;

                if (a.CustomProperties.ContainsKey("map"))
                {
                    newRoomButton.transform.Find("Map/Name").GetComponent<Text>().text = maps[(int)a.CustomProperties["map"]].name;
                }
                else newRoomButton.transform.Find("Map/Name").GetComponent<Text>().text = "----";

                newRoomButton.GetComponent<Button>().onClick.AddListener(delegate { JoinRoom(newRoomButton.transform); });
            }

            base.OnRoomListUpdate(roomList);
        }

        private void VerifyUsername()
        {
            if (string.IsNullOrEmpty(usernameField.text))
            {
                myProfile.username = "Player_" + Random.Range(100, 1000);
            }
            else
            {
                myProfile.username = usernameField.text;
            }
        }

        public void JoinRoom(Transform p_button)
        {
            VerifyUsername();
            string t_roomName = p_button.Find("Name").GetComponent<Text>().text;
            PhotonNetwork.JoinRoom(t_roomName);
        }

        public void StartGame()
        {
            VerifyUsername();

            if(PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                Debug.Log("Level Loaded");
                PhotonNetwork.LoadLevel(maps[currentmap].scene);
            }
        }
    }
}
