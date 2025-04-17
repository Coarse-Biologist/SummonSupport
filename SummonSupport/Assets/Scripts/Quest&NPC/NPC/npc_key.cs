using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class npc_key<TKey, TValue>
{
    [SerializeField] public TKey Key;
    [SerializeField] public TValue Value;
}
