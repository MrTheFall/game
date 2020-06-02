using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameManagers
{
    static AmmoPool ammoPool;
    public static AmmoPool GetAmmoPool()
    {
        if (ammoPool = null)
        {
            ammoPool = Resources.Load("AmmoPool") as AmmoPool;
            ammoPool.Init();
        }
        return ammoPool;
    }
}
