using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
   public void TakeDamage(float _amnt)
    {
        Debug.Log("Enemy was hit with " + _amnt.ToString() + " of damage");
    }
}
