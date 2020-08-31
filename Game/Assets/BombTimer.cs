using FPSGame;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombTimer : MonoBehaviour
{
    public float bombTimer;
    bool isExploded = false;
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
        manager.BombExplosionRoundEnd();
    }
}
