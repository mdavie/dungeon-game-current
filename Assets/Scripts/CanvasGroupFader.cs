using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupFader : MonoBehaviour
{
    public float fadeDuration = 0.35f;
    CanvasGroup _cg;
    Coroutine _co;

    void Awake() { _cg = GetComponent<CanvasGroup>(); }

    public void ShowImmediate()
    {
        if (!_cg) _cg = GetComponent<CanvasGroup>();
        _cg.alpha = 1f; _cg.blocksRaycasts = true; _cg.interactable = true;
    }

    public void HideImmediate()
    {
        if (!_cg) _cg = GetComponent<CanvasGroup>();
        _cg.alpha = 0f; _cg.blocksRaycasts = false; _cg.interactable = false;
    }

    public void FadeIn()  { StartFade(1f, true); }
    public void FadeOut() { StartFade(0f, false); }

    void StartFade(float target, bool enable)
    {
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(Fade(target, enable));
    }

    IEnumerator Fade(float target, bool enableAfter)
    {
        if (!_cg) _cg = GetComponent<CanvasGroup>();
        float start = _cg.alpha;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            _cg.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }
        _cg.alpha = target;
        _cg.blocksRaycasts = enableAfter;
        _cg.interactable   = enableAfter;
    }
}
