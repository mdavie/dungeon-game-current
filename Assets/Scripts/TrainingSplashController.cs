using UnityEngine;
using UnityEngine.UI; // if you use Text; swap to TMP if desired

[RequireComponent(typeof(CanvasGroupFader))]
public class TrainingSplashController : MonoBehaviour
{
    [Header("UI (optional)")]
    public Text statusText;                  // or TMP_Text
    [TextArea] public string idleText     = "Press Start to begin ERP training";
    [TextArea] public string trainingText = "Training… Focus on the flashes";
    [TextArea] public string doneText     = "Training complete!";

    [Header("Behavior")]
    public bool hideWhenTrained = true;
    public bool showOnStart = true;

    CanvasGroupFader _fader;

    void Awake() { _fader = GetComponent<CanvasGroupFader>(); }

    void OnEnable()
    {
        if (BCITrainingState.Instance)
        {
            BCITrainingState.Instance.onTrainingStarted.AddListener(HandleTrainingStarted);
            BCITrainingState.Instance.onTrainingCompleted.AddListener(HandleTrainingCompleted);
        }

        if (showOnStart)
        {
            _fader.ShowImmediate();
            SetText(idleText);
        }
        else
        {
            if (BCITrainingState.Instance && BCITrainingState.Instance.IsTrained)
                _fader.HideImmediate();
        }
    }

    void OnDisable()
    {
        if (BCITrainingState.Instance)
        {
            BCITrainingState.Instance.onTrainingStarted.RemoveListener(HandleTrainingStarted);
            BCITrainingState.Instance.onTrainingCompleted.RemoveListener(HandleTrainingCompleted);
        }
    }

    public void HandleTrainingStarted()
    {
        SetText(trainingText);
        _fader.FadeIn(); // ensure visible during training
    }

    public void HandleTrainingCompleted()
    {
        SetText(doneText);
        if (hideWhenTrained) _fader.FadeOut();
    }

    void SetText(string s)
    {
        if (statusText) statusText.text = s;
    }
}
