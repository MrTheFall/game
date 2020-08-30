using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    private bool onBombPlant = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (onBombPlant)
            {
                //PlantBomb(); !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            }
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "BombPlant")
        {
            onBombPlant = true;
        }
    }
    void OnTriggerExit(Collider collider)
    {
        if (collider.tag == "BombPlant")
        {
            onBombPlant = false;
        }
    }
}
