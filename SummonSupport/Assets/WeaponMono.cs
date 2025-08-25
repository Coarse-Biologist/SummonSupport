using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponMono : MonoBehaviour
{

    [field: SerializeField] public WeaponUseTypes useAppearanceType { private set; get; }

    [field: SerializeField] public float weaponUseSteps { private set; get; } = 10;

    private int weaponStepCounter = 0;
    public void UseWeapon()
    {


        if (useAppearanceType == WeaponUseTypes.LightSword)
        {
            InvokeRepeating("CreateWeaponMotionPath", 0f, .02f);
            Invoke("EndWeaponUse", .2f);
        }
    }
    private void CreateWeaponMotionPath()
    {
        gameObject.transform.Rotate(-Vector3.back, 10, Space.Self);
        //if (weaponStepCounter < weaponUseSteps / 2)
        //    transform.position = new Vector2(transform.position.x, transform.position.y + .02f);
        //else transform.position = new Vector2(transform.position.x, transform.position.y - .01f);

        weaponStepCounter += 1;

    }
    private void EndWeaponUse()
    {
        CancelInvoke("CreateWeaponMotionPath");
        gameObject.transform.rotation = Quaternion.identity;
    }
}

