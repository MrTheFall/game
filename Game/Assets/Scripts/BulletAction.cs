using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletAction : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider hitInfo)
    {
        if(hitInfo.gameObject.name != "Gun")
        {
            Destroy(gameObject); //Here must be enemy hit logic
            Debug.Log(hitInfo.gameObject);
        }
    }
}
