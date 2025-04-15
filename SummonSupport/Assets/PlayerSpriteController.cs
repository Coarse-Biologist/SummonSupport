using UnityEngine;

public class PlayerSpriteController : MonoBehaviour
{
    private SpriteRenderer sr;
    [SerializeField] private Sprite upSprite;
    [SerializeField] private Sprite rightSprite;
    [SerializeField] private Sprite downSprite;
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite upRightSprite;
    [SerializeField] private Sprite rightDownSprite;
    [SerializeField] private Sprite downLeftSprite;
    [SerializeField] private Sprite leftUpSprite;




    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetPlayerSprite(float angle)
    {
        if (angle >= -22.5f && angle < 22.5f) sr.sprite = rightSprite;
        else if (angle >= 22.5f && angle < 67.5f) sr.sprite = upRightSprite;
        else if (angle >= 67.5f && angle < 112.5f) sr.sprite = upSprite;
        else if (angle >= 112.5f && angle < 157.5f) sr.sprite = leftUpSprite;
        else if (angle >= 157.5f || angle < -157.5f) sr.sprite = leftSprite;
        else if (angle >= -157.5f && angle < -112.5f) sr.sprite = downLeftSprite;
        else if (angle >= -112.5f && angle < -67.5f) sr.sprite = downSprite;
        else if (angle >= -67.5f && angle < -22.5f) sr.sprite = rightDownSprite;
    }
}
