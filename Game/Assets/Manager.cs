using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace FPSGame
{
    public class Manager : MonoBehaviour
    {
        public string player_prefab;
        public Transform[] spawnpoints;

        private void Start()
        {
            Spawn();
        }

        public void Spawn()
        {
            Transform t_spawn = spawnpoints[Random.Range(0, spawnpoints.Length)]; 
            PhotonNetwork.Instantiate(player_prefab, t_spawn.position, t_spawn.rotation);
        }
    }
}