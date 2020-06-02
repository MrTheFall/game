using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Managers/AmmoPool")]
public class AmmoPool : ScriptableObject
{
    public List<Ammo> all_ammo = new List<Ammo>();
    Dictionary<string, Ammo> ammoDictionary = new Dictionary<string, Ammo>();

    public void Init()
    {
        for (int i = 0; i < all_ammo.Count; i++)
        {
            Ammo a = Instantiate(all_ammo[i]);
            a.name = all_ammo[i].name;
            ammoDictionary.Add(a.name, a);
        }
    }
    
    public Ammo GetAmmo(string id)
    {
        Ammo r = null;
        ammoDictionary.TryGetValue(id, out r);
        return r;
    }
}
