using UnityEngine;
namespace Quest
{
    [System.Serializable]
    public class Questline_Pair
    {
        [field: SerializeField] public Quest_SO CompletedQuest;
        [field: SerializeField] public Quest_SO NextQuest;

    }
}
