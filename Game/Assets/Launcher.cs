using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace FPSGame
{
    public class Launcher : MonoBehaviourPunCallbacks
    {
        public void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            Connect();
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("Joined");
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
            PhotonNetwork.CreateRoom("");
            Debug.Log("Created new room");
        }

        public void StartGame()
        {
            if(PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                Debug.Log("Level Loaded");
                PhotonNetwork.LoadLevel(1);
            }
        }
    }
}
