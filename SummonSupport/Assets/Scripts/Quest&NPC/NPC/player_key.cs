using UnityEngine;

[System.Serializable]
public class player_key<TKey, TValue>
{
    [SerializeField] public TKey Key;
    [SerializeField] public TValue Value;
    [SerializeField] public DialogueResult result;


}
