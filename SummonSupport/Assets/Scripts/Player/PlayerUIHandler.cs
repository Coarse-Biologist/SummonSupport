using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerUIHandler : MonoBehaviour
{
    [SerializeField] public UIDocument uiDoc;
    private VisualElement root;
    private VisualElement minionHPBars;
    private VisualElement playerUI;

    [SerializeField] List<GameObject> minions = new List<GameObject>();

    void Awake()
    {
        root = uiDoc.rootVisualElement;
        playerUI = root.Q<VisualElement>("MainUI");
        minionHPBars = playerUI.Q<VisualElement>("MinionBarSlots");
        foreach (GameObject minion in minions)
        {
            AddMinionHP(minion);
        }
    }

    public ProgressBar AddMinionHP(GameObject minion)
    {
        ProgressBar minionHP = minionHPBars.Q<ProgressBar>("MinionBar");
        minionHP.title = $"{minion.GetComponent<LivingBeing>().Name} HP";
        minionHPBars.Add(minionHP);
        return minionHP;
    }

    public void AlterSummonHP(ProgressBar minionHPbar, int value)
    {
        minionHPbar.value = minionHPbar.value += value;
    }
}
