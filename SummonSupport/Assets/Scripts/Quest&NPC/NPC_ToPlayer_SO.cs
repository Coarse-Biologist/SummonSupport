using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName = "NPC_ToPlayer_Dialogue")]

public class NPC_ToPlayer_SO : ScriptableObject
{
    [SerializeField] public string key;
    [SerializeField] public List<string> value;
}
