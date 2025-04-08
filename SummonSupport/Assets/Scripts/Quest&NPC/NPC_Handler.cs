using UnityEngine;

public class NPC_Handler : MonoBehaviour
{
[SerializeField] NPC_SO npcData;

    void Awake()
    {
GetComponentInChildren<SpriteRenderer>().sprite = npcData.NPC_Sprite;
    }
}
