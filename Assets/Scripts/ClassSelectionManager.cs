using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// Class picker with per-class actions.
/// - You can invoke multiple functions when a class is selected (and when it’s deselected).
/// - You can also auto-toggle GameObjects on select/deselect (e.g., show ManualButton, hide PlayerWeapon).
///
/// HOW TO USE (example):
/// 1) Add this script to an empty GameObject under your HUD (e.g., "ClassSelection").
/// 2) Fill the four class sections. For Wizard:
///    - Set "Tint" to the Image you want to swap/tint.
///    - Put your ManualButton in "Set Active On Select".
///    - Put your PlayerWeapon in "Set Inactive On Select".
///    - (Optional) In "On Selected" add extra methods to call (you can add multiple).
///    - (Optional) In "On Deselected" put PlayerWeapon in "Set Active On Deselect" to show it again.
/// 3) Press 1/2/3/4 (or W) to switch classes, or call SelectWarrior/Rogue/Cleric/Wizard from UI Buttons.
public class ClassSelectionManager : MonoBehaviour
{
    public enum ClassType { Warrior, Rogue, Cleric, Wizard }

    [System.Serializable]
    public class ClassConfig
    {
        [Header("Identity (optional)")]
        public string displayName;

        [Tooltip("(Optional) Root object for organization.")]
        public GameObject root;

        [Header("Icon Visuals")]
        [Tooltip("Image to color/swap when selected/deselected (portrait/frame).")]
        public Image tint;
        public Sprite normalSprite;    // shown when NOT selected (optional)
        public Sprite selectedSprite;  // shown when selected (optional)
        public Color selectedColor   = Color.white;
        public Color unselectedColor = new Color(1f, 1f, 1f, 0.35f);
        [Tooltip("Shown only when selected (e.g., glow/border). Optional.")]
        public GameObject selectedFrame;

        [Header("Auto Toggles (executed in addition to events)")]
        [Tooltip("GameObjects to SetActive(true) when this class is selected.")]
        public GameObject[] setActiveOnSelect;
        [Tooltip("GameObjects to SetActive(false) when this class is selected.")]
        public GameObject[] setInactiveOnSelect;
        [Tooltip("GameObjects to SetActive(true) when this class is deselected.")]
        public GameObject[] setActiveOnDeselect;
        [Tooltip("GameObjects to SetActive(false) when this class is deselected.")]
        public GameObject[] setInactiveOnDeselect;

        [Header("Callbacks")]
        [Tooltip("All functions to call when this class is selected (can add many).")]
        public UnityEvent onSelected;
        [Tooltip("All functions to call when this class is deselected (can add many).")]
        public UnityEvent onDeselected;
    }

    [Header("Classes")]
    public ClassConfig warrior;
    public ClassConfig rogue;
    public ClassConfig cleric;
    public ClassConfig wizard;

    [Header("Initial State")]
    public ClassType initial = ClassType.Warrior;

    [Header("Debug")]
    public bool logTransitions = true;

    public ClassType Current { get; private set; }

    void Start()
    {
        // Initialize visuals & toggles for the initial class
        Current = initial;
        ApplyAll();
    }

    void Update()
    {
        // Keyboard shortcuts for quick testing
        if (Input.GetKeyDown(KeyCode.Alpha1)) Set(ClassType.Warrior);
        if (Input.GetKeyDown(KeyCode.Alpha2)) Set(ClassType.Rogue);
        if (Input.GetKeyDown(KeyCode.Alpha3)) Set(ClassType.Cleric);
        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.W)) Set(ClassType.Wizard);
    }

    // Public methods you can hook from UI Buttons
    public void SelectWarrior() => Set(ClassType.Warrior);
    public void SelectRogue()   => Set(ClassType.Rogue);
    public void SelectCleric()  => Set(ClassType.Cleric);
    public void SelectWizard()  => Set(ClassType.Wizard);

    public void Set(ClassType next)
    {
        if (next == Current) return;

        // Deselect previous
        var prevCfg = GetConfig(Current);
        if (prevCfg != null)
        {
            ApplyVisual(prevCfg, isSelected: false);
            DoToggles(prevCfg, onSelect: false);
            SafeInvoke(prevCfg.onDeselected, $"{prevCfg.displayName} OnDeselected");
        }

        // Select new
        var nextCfg = GetConfig(next);
        if (nextCfg != null)
        {
            ApplyVisual(nextCfg, isSelected: true);
            DoToggles(nextCfg, onSelect: true);
            SafeInvoke(nextCfg.onSelected, $"{nextCfg.displayName} OnSelected");
        }

        if (logTransitions)
            Debug.Log($"[ClassSelection] {Current} -> {next}");

        Current = next;
    }

    // --- Helpers ---

    ClassConfig GetConfig(ClassType t)
    {
        return t switch
        {
            ClassType.Warrior => warrior,
            ClassType.Rogue   => rogue,
            ClassType.Cleric  => cleric,
            ClassType.Wizard  => wizard,
            _ => null
        };
    }

    void ApplyAll()
    {
        // Make exactly one selected (Current) and the others deselected
        foreach (var tup in new (ClassType, ClassConfig)[] {
            (ClassType.Warrior, warrior), (ClassType.Rogue, rogue),
            (ClassType.Cleric, cleric),   (ClassType.Wizard, wizard)
        })
        {
            bool sel = tup.Item1 == Current;
            var cfg = tup.Item2;
            if (cfg == null) continue;

            ApplyVisual(cfg, sel);
            DoToggles(cfg, sel);
            if (sel) SafeInvoke(cfg.onSelected, $"{cfg.displayName} OnSelected(init)");
            else     SafeInvoke(cfg.onDeselected, $"{cfg.displayName} OnDeselected(init)");
        }
    }

    void ApplyVisual(ClassConfig cfg, bool isSelected)
    {
        if (cfg.tint)
        {
            // Swap sprite if provided
            if (isSelected && cfg.selectedSprite) cfg.tint.sprite = cfg.selectedSprite;
            if (!isSelected && cfg.normalSprite)  cfg.tint.sprite = cfg.normalSprite;

            // Apply tint
            cfg.tint.color = isSelected ? cfg.selectedColor : cfg.unselectedColor;
        }

        if (cfg.selectedFrame)
            cfg.selectedFrame.SetActive(isSelected);
    }

    void DoToggles(ClassConfig cfg, bool onSelect)
    {
        if (onSelect)
        {
            SetActiveArray(cfg.setActiveOnSelect,   true);
            SetActiveArray(cfg.setInactiveOnSelect, false);
        }
        else
        {
            SetActiveArray(cfg.setActiveOnDeselect,   true);
            SetActiveArray(cfg.setInactiveOnDeselect, false);
        }
    }

    void SetActiveArray(GameObject[] arr, bool value)
    {
        if (arr == null) return;
        for (int i = 0; i < arr.Length; i++)
        {
            var go = arr[i];
            if (go) go.SetActive(value);
        }
    }

    void SafeInvoke(UnityEvent evt, string label)
    {
        try { evt?.Invoke(); }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[ClassSelection] Exception in {label}: {ex.Message}");
        }
    }
}
