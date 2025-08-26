using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;

public class WeaponMono : MonoBehaviour
{

    [field: SerializeField] public WeaponUseTypes useAppearanceType { private set; get; }

    [field: SerializeField] public float weaponUseSpeed { private set; get; } = .8f;

    [field: SerializeField] public Sprite WeaponImage { private set; get; } = null;
    private SplineAnimate splineAnimator;
    private Vector2 startLoc;

    public void UseWeapon(Quaternion rotation)
    {
        startLoc = transform.localPosition;

        if (gameObject.TryGetComponent<SplineAnimate>(out SplineAnimate animator))
        {
            splineAnimator = animator;
            Debug.Log($"animator's spline = {animator.Container.name}");
            animator.Container.transform.rotation = new Quaternion(0, 0, rotation.z, 0);
            if (gameObject.TryGetComponent<SpriteRenderer>(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer.sprite = WeaponImage;
            }
            animator.Duration = weaponUseSpeed;
            animator.Play();
            Invoke("EndWeaponUse", weaponUseSpeed);
        }
    }

    private void EndWeaponUse()
    {
        Debug.Log("Ending weapon use");
        splineAnimator.Restart(true);
        splineAnimator.Container.transform.rotation = Quaternion.identity;
        //transform.localPosition = startLoc;
    }

}

