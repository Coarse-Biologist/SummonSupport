using UnityEngine;

[CreateAssetMenu(fileName = "Weapon_SO", menuName = "WeaponUse/Weapons")]

public class WeaponInfo : ScriptableObject
{
    [field: SerializeField] public WeaponUseTypes WeaponType { private set; get; }
    [field: SerializeField] public Sprite WeaponSprite { private set; get; }
    [field: SerializeField] public GameObject animationSplineObject { private set; get; }
}


