using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UIElements;

/// <summary>
/// Lets a controller (PS5, Xbox, ...) drive the menus. Navigation itself comes from the EventSystem's
/// InputSystemUIInputModule (stick / d-pad to move, A / Cross to press, B / Circle to cancel); this makes sure
/// something is selected for it to move from, both in the start screen (uGUI) and in the in-game UI Toolkit menus.
/// Menus in this game unlock the cursor while open, so an unlocked cursor is what counts as "a menu is open".
/// Created once at startup and kept across scenes.
/// </summary>
public class MenuNavigation : MonoBehaviour
{
    /// <summary>Set on the in-game UI root while the controller is in use, so focus styles only show for it (see CraftingMenu.uss).</summary>
    public const string ControllerNavigationClass = "controller-navigation";

    private const float StickThreshold = 0.5f;

    private static MenuNavigation instance;

    /// <summary>True from the last controller (or arrow key) navigation input until the mouse is used again.</summary>
    public static bool UsingController { private set; get; }

    private bool stickWasHeld;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        if (instance != null) return;
        GameObject go = new GameObject(nameof(MenuNavigation));
        DontDestroyOnLoad(go);
        instance = go.AddComponent<MenuNavigation>();
    }

    void Update()
    {
        if (ControllerNavigatedThisFrame()) UsingController = true;
        else if (PointerUsedThisFrame()) UsingController = false;

        // A clicked start screen button stays selected, which now shows as highlighted; mouse users don't need that.
        if (!UsingController) DeselectButton();

        VisualElement root = GameplayUIRoot();
        root?.EnableInClassList(ControllerNavigationClass, UsingController);

        // A menu that closes by hiding keeps its focused button, which would still take A / Cross presses during play.
        if (root?.panel?.focusController.focusedElement is VisualElement focused && !IsNavigable(focused)) focused.Blur();

        // Menus are rebuilt as they are used (their buttons are destroyed and made again), which drops the focus.
        if (UsingController && UnityEngine.Cursor.lockState != CursorLockMode.Locked && !HasMenuFocus()) FocusFirstMenuItem();
    }

    #region Public helpers

    /// <summary>Focuses the first button or toggle a controller can reach inside <paramref name="container"/>, if the controller is in use.</summary>
    public static bool FocusFirst(VisualElement container)
    {
        if (!UsingController || container == null) return false;
        VisualElement first = container.Query<VisualElement>().Where(IsNavigable).First();
        if (first == null) return false;
        first.Focus();
        return true;
    }

    /// <summary>Selects <paramref name="selectable"/> for controller navigation, if the controller is in use.</summary>
    public static void Select(UnityEngine.UI.Selectable selectable)
    {
        if (!UsingController || selectable == null || EventSystem.current == null) return;
        EventSystem.current.SetSelectedGameObject(selectable.gameObject);
    }

    /// <summary>
    /// Links <paramref name="items"/> to each other by where they are on screen. Unity's automatic navigation
    /// compares world positions, which go wrong across canvases with different render modes (the start screen's
    /// main buttons and its host/join column).
    /// </summary>
    public static void LinkByScreenPosition(IReadOnlyList<UnityEngine.UI.Selectable> items)
    {
        foreach (UnityEngine.UI.Selectable item in items)
        {
            if (item == null) continue;
            item.navigation = new UnityEngine.UI.Navigation
            {
                mode = UnityEngine.UI.Navigation.Mode.Explicit,
                selectOnUp = Nearest(item, Vector2.up, items),
                selectOnDown = Nearest(item, Vector2.down, items),
                selectOnLeft = Nearest(item, Vector2.left, items),
                selectOnRight = Nearest(item, Vector2.right, items),
            };
        }
    }

    // Same scoring as Unity's automatic navigation, but in screen space.
    private static UnityEngine.UI.Selectable Nearest(UnityEngine.UI.Selectable from, Vector2 direction, IReadOnlyList<UnityEngine.UI.Selectable> items)
    {
        Vector2 origin = ScreenPosition(from);
        UnityEngine.UI.Selectable best = null;
        float bestScore = 0f;
        foreach (UnityEngine.UI.Selectable item in items)
        {
            if (item == null || item == from) continue;
            Vector2 offset = ScreenPosition(item) - origin;
            float dot = Vector2.Dot(direction, offset);
            if (dot <= 0f) continue;
            float score = dot / offset.sqrMagnitude;
            if (score <= bestScore) continue;
            bestScore = score;
            best = item;
        }
        return best;
    }

    private static Vector2 ScreenPosition(UnityEngine.UI.Selectable selectable)
    {
        Canvas canvas = selectable.GetComponentInParent<Canvas>();
        Camera camera = canvas != null && canvas.rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.rootCanvas.worldCamera : null;
        RectTransform rect = (RectTransform)selectable.transform;
        return RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));
    }

    /// <summary>True on the frame the menu cancel input (B / Circle, Escape) is pressed.</summary>
    public static bool CancelPressedThisFrame()
    {
        if (EventSystem.current == null || EventSystem.current.currentInputModule is not InputSystemUIInputModule module) return false;
        InputAction cancel = module.cancel != null ? module.cancel.action : null;
        return cancel != null && cancel.WasPressedThisFrame();
    }

    /// <summary>True while a menu item has controller focus, i.e. the controller's buttons belong to the menu, not to gameplay.</summary>
    public static bool HasMenuFocus()
    {
        VisualElement root = GameplayUIRoot();
        if (root != null && root.panel != null && IsNavigable(root.panel.focusController.focusedElement as VisualElement)) return true;

        GameObject selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        return selected != null && selected.activeInHierarchy
            && selected.TryGetComponent(out UnityEngine.UI.Selectable selectable) && selectable.IsInteractable();
    }

    #endregion

    #region Focus

    private static VisualElement GameplayUIRoot()
    {
        UI_DocHandler handler = UI_DocHandler.Instance;
        return handler != null && handler.ui != null ? handler.ui.rootVisualElement : null;
    }

    private static void FocusFirstMenuItem()
    {
        if (FocusFirst(GameplayUIRoot())) return;
        UnityEngine.UI.Selectable first = PreferredSelectable() ?? TopmostSelectable();
        if (first != null) Select(first);
    }

    // The menu can name the item to start on (the start screen names its Start button).
    private static UnityEngine.UI.Selectable PreferredSelectable()
    {
        GameObject preferred = EventSystem.current != null ? EventSystem.current.firstSelectedGameObject : null;
        return preferred != null && preferred.TryGetComponent(out UnityEngine.UI.Selectable selectable) && IsSelectable(selectable) ? selectable : null;
    }

    // The start screen panels grow down from the top, so the topmost item is the first one.
    private static UnityEngine.UI.Selectable TopmostSelectable()
    {
        UnityEngine.UI.Selectable best = null;
        float bestY = float.MinValue;
        foreach (UnityEngine.UI.Selectable selectable in UnityEngine.UI.Selectable.allSelectablesArray)
        {
            if (!IsSelectable(selectable)) continue;
            float y = ScreenPosition(selectable).y;
            if (y <= bestY) continue;
            bestY = y;
            best = selectable;
        }
        return best;
    }

    private static bool IsSelectable(UnityEngine.UI.Selectable selectable)
    {
        return selectable.IsInteractable() && selectable.isActiveAndEnabled && selectable.navigation.mode != UnityEngine.UI.Navigation.Mode.None;
    }

    private static void DeselectButton()
    {
        GameObject selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        if (selected != null && selected.TryGetComponent(out UnityEngine.UI.Button _)) EventSystem.current.SetSelectedGameObject(null);
    }

    private static bool IsNavigable(VisualElement element)
    {
        if (element == null || element.panel == null || !element.focusable || !element.enabledInHierarchy) return false;
        if (element is not UnityEngine.UIElements.Button && element is not Toggle) return false;
        for (VisualElement v = element; v != null; v = v.parent)
        {
            // Inline styles first: a panel hidden this frame has not been restyled yet.
            DisplayStyle display = v.style.display.keyword == StyleKeyword.Undefined ? v.style.display.value : v.resolvedStyle.display;
            if (display == DisplayStyle.None || v.resolvedStyle.visibility == Visibility.Hidden) return false;
        }
        return true;
    }

    #endregion

    #region Input

    private bool ControllerNavigatedThisFrame()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && (keyboard.upArrowKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame
            || keyboard.leftArrowKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)) return true;

        Gamepad pad = Gamepad.current;
        if (pad == null) return false;

        bool stickHeld = pad.leftStick.ReadValue().magnitude > StickThreshold;
        bool stickPushed = stickHeld && !stickWasHeld;
        stickWasHeld = stickHeld;

        return stickPushed
            || pad.dpad.up.wasPressedThisFrame || pad.dpad.down.wasPressedThisFrame
            || pad.dpad.left.wasPressedThisFrame || pad.dpad.right.wasPressedThisFrame
            || pad.buttonSouth.wasPressedThisFrame || pad.startButton.wasPressedThisFrame;
    }

    private static bool PointerUsedThisFrame()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return false;
        return mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame
            || mouse.delta.ReadValue().sqrMagnitude > 4f;
    }

    #endregion
}
