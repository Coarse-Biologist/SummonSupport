using System;
using System.Collections.Generic;
using UnityEngine;

public class MovementScript : MonoBehaviour
{

    [field: SerializeField] public float MovementSpeed { private set; get; } = .5f;
    [field: SerializeField] public float AttackSpeed { private set; get; } = 1f;

    [field: SerializeField] public float DashBoost { private set; get; } = 10f;
    [field: SerializeField] public float DashCoolDown { private set; get; } = 1f;
    [field: SerializeField] public float DashDuration { private set; get; } = .1f;
    [field: SerializeField] public float Weight { private set; get; } = .1f;
    public Dictionary<MovementAttributes, (Func<float> Get, Action<float> Set)> MovementAttributeDict = new();

    void Start()
    {
        InitializeAttributeDict();
    }
    public void SetMovementAttribute(MovementAttributes attribute, float newValue)
    {
        switch (attribute)
        {
            case MovementAttributes.MovementSpeed:
                MovementSpeed = Mathf.Max(newValue, 0);
                break;
            case MovementAttributes.DashBoost:
                DashBoost = Mathf.Max(newValue, 0);
                break;
            case MovementAttributes.DashCooldown:
                DashCoolDown = Mathf.Max(newValue, 0);
                break;
            case MovementAttributes.DashDuration:
                DashDuration = Mathf.Max(newValue, 0);
                break;
            case MovementAttributes.AttackSpeed:
                AttackSpeed = Mathf.Max(newValue, 0);
                break;
            case MovementAttributes.Weight:
                Weight = Mathf.Max(newValue, 0);
                break;
        }
    }
    void InitializeAttributeDict()
    {
        MovementAttributeDict = new Dictionary<MovementAttributes, (Func<float> Get, Action<float> Set)>
            {
                { MovementAttributes.MovementSpeed,          (() => MovementSpeed,             v => MovementSpeed = v) },
                { MovementAttributes.DashBoost,              (() => DashBoost,                 v => DashBoost = v) },
                { MovementAttributes.DashCooldown,           (() => DashCoolDown,              v => DashCoolDown = v) },
                { MovementAttributes.DashDuration,           (() => DashDuration,              v => DashDuration = v) },
                { MovementAttributes.Weight,                 (() => Weight,                    v => Weight = v) },
                { MovementAttributes.AttackSpeed,            (() => AttackSpeed,               v => AttackSpeed = v) },


            };
    }

    public float GetMovementAttribute(MovementAttributes movementAttribute)
    {
        return MovementAttributeDict[movementAttribute].Get();
    }
}
