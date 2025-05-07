using UnityEngine;
using UnityEngine.UIElements;


public class UI_DocHandler : MonoBehaviour
{
    public static UI_DocHandler Instance { private set; get; }
    [SerializeField] public VisualTreeAsset UIPrefabAssets;
    public UIDocument ui;

    void Awake()
    {
        ui = GetComponent<UIDocument>();
        Instance = this;
    }
}
