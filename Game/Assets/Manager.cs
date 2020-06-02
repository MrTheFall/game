using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace FPSGame
{
    public class Manager : MonoBehaviour
    {
        public string player_prefab;
        public Transform spawnpoint;

        private void Start()
        {
            Spawn();
        }

        public void Spawn()
        {
            PhotonNetwork.Instantiate(player_prefab, spawnpoint.position, spawnpoint.rotation);
        }
    }
}