using UnityEngine;
using UnityEngine.Events;

public class BCITrainingState : MonoBehaviour
{
    public static BCITrainingState Instance { get; private set; }

    [Header("Events")]
    public UnityEvent onTrainingStarted;
    public UnityEvent onTrainingCompleted;

    [Header("Debug")]
    public bool allowDebugKeys = true;  // F9 start, F10 complete

    public bool IsTraining { get; private set; }
    public bool IsTrained  { get; private set; }

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (!allowDebugKeys) return;
        if (Input.GetKeyDown(KeyCode.F9)) StartTraining();
        if (Input.GetKeyDown(KeyCode.F10)) MarkTrained();
    }

    // Call from your "Start Training" button or ERP component
    public void StartTraining()
    {
        if (IsTraining || IsTrained) return;
        IsTraining = true;
        onTrainingStarted?.Invoke();
    }

    // Call from your ERP "Training Complete" event
    public void MarkTrained()
    {
        if (IsTrained) return;
        IsTraining = false;
        IsTrained  = true;
        onTrainingCompleted?.Invoke();
    }

    // Optional reset (e.g., if you want to retrain later)
    public void ResetState()
    {
        IsTraining = false;
        IsTrained  = false;
    }
}
