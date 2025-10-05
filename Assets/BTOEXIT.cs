// Assets/Scripts/UI/TrainingDismissOnKey.cs
using UnityEngine;

public class TrainingDismissOnKey : MonoBehaviour
{
    [Header("Trigger")]
    public KeyCode key = KeyCode.B;

    [Header("Behavior")]
    public bool markTrained = true;     // calls BCITrainingState.MarkTrained() if present
    public bool fadeOut = true;         // fade if a CanvasGroup is on this object
    public float fadeDuration = 0.3f;

    CanvasGroup cg;
    bool fading;
    float t;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>(); // optional
        if (cg) { cg.alpha = 1f; cg.blocksRaycasts = true; cg.interactable = true; }
    }

    void Update()
    {
        if (Input.GetKeyDown(key))
            Dismiss();

        if (fading && cg)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = 1f - Mathf.Clamp01(t / fadeDuration);
            if (t >= fadeDuration)
            {
                fading = false;
                gameObject.SetActive(false);
            }
        }
    }

    // You can also hook this to a UI Button OnClick
    public void Dismiss()
    {
        // mark trained if you’re using the state helper (safe if not present)
        if (markTrained && BCITrainingState.Instance != null)
            BCITrainingState.Instance.MarkTrained();

        if (fadeOut && cg)
        {
            cg.blocksRaycasts = false;
            cg.interactable = false;
            t = 0f;
            fading = true;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
