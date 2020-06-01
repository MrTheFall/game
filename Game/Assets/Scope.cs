using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scope : MonoBehaviour
{
    public Animator animator;
    public GameObject Rifle;
    public GameObject Pistol;

    public string RifleBool;
    public string PistolBool;

    // Update is called once per frame
    void Update()
    {
        if (gameObject.transform.childCount > 0)
        {
            if (gameObject.transform.GetChild(0).name == Rifle.transform.name)
            {
                if (Input.GetButtonDown("Fire2"))
                {
                    animator.SetBool(RifleBool, true);
                }
                if (Input.GetButtonUp("Fire2"))
                {
                    animator.SetBool(RifleBool, false);
                }
            }
            if (gameObject.transform.GetChild(0).name == Pistol.transform.name)
            {
                if (Input.GetButtonDown("Fire2"))
                {
                    animator.SetBool(PistolBool, true);
                }
                if (Input.GetButtonUp("Fire2"))
                {
                    animator.SetBool(PistolBool, false);
                }
            }
        }
    }
}
