using UnityEngine;

public class CreatureSpriteController : MonoBehaviour
{
    private SpriteRenderer sr;
    [SerializeField] private Sprite NSprite;
    [SerializeField] private Sprite ESprite;
    [SerializeField] private Sprite SSprite;
    [SerializeField] private Sprite WSprite;
    [SerializeField] private Sprite NESprite;
    [SerializeField] private Sprite SESprite;
    [SerializeField] private Sprite SWSprite;
    [SerializeField] private Sprite NWSprite;
    [field: SerializeField] public bool unfinished { get; private set; } = false;
    [field: SerializeField] public bool IsoOnly { get; private set; } = false;






    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetSpriteDirection(float angle)
    {
        if (IsoOnly)
        {
            if (angle >= -180 && angle < -90) sr.sprite = SWSprite;
            else if (angle >= -90 && angle < 0) sr.sprite = SESprite;
            else if (angle >= 90 && angle < 180) sr.sprite = NWSprite;
            else if (angle >= 0 && angle < 90) sr.sprite = NESprite;

        }
        else
        {
            if (!unfinished)
            {
                if (angle >= -22.5f && angle < 22.5f) sr.sprite = ESprite;
                else if (angle >= 22.5f && angle < 67.5f) sr.sprite = NESprite;
                else if (angle >= 67.5f && angle < 112.5f) sr.sprite = NSprite;
                else if (angle >= 112.5f && angle < 157.5f) sr.sprite = NWSprite;
                else if (angle >= 157.5f || angle < -157.5f) sr.sprite = WSprite;
                else if (angle >= -157.5f && angle < -112.5f) sr.sprite = SWSprite;
                else if (angle >= -112.5f && angle < -67.5f) sr.sprite = SSprite;
                else if (angle >= -67.5f && angle < -22.5f) sr.sprite = SESprite;
            }
        }

    }
}
