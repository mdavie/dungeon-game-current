using UnityEngine;

/// <summary>
/// Rotates a sprite (or any Transform) to face the scene camera.
/// Supports full billboard or Y-axis-only (upright) billboard.
/// </summary>
[ExecuteAlways]
public class SpriteBillboard : MonoBehaviour
{
    public enum BillboardMode { FaceCamera, YAxisOnly }

    [Header("Behavior")]
    public BillboardMode mode = BillboardMode.YAxisOnly;
    [Tooltip("Optional world-space rotation offset applied after billboarding.")]
    public Vector3 rotationOffsetEuler;

    [Header("Target Camera (optional)")]
    [Tooltip("If not set, the script finds a camera automatically.")]
    public Camera targetCamera;

    // Cache to avoid repeated searches
    private Transform _tr;

    private void OnEnable()
    {
        _tr = transform;
        EnsureCamera();
    }

    private void LateUpdate()
    {
        if (!EnsureCamera()) return;

        var camTr = targetCamera.transform;

        if (mode == BillboardMode.FaceCamera)
        {
            // Look directly at camera
            _tr.forward = ( _tr.position - camTr.position ).normalized;
        }
        else // YAxisOnly
        {
            // Keep object upright; rotate only around Y so it faces camera horizontally
            Vector3 toCam = _tr.position - camTr.position;
            toCam.y = 0f;
            if (toCam.sqrMagnitude > 0.0001f)
            {
                _tr.forward = toCam.normalized;
            }
        }

        if (rotationOffsetEuler != Vector3.zero)
        {
            _tr.rotation = _tr.rotation * Quaternion.Euler(rotationOffsetEuler);
        }
    }

    /// <summary>
    /// Ensures we have a camera reference. Returns true if available.
    /// </summary>
    private bool EnsureCamera()
    {
        if (targetCamera != null) return true;

        // Try the scene/main camera first
        targetCamera = Camera.main;
        if (targetCamera != null) return true;

        // Fallback: find any camera (Unity 2022.2+ API first)
#if UNITY_2022_2_OR_NEWER
        targetCamera = Object.FindFirstObjectByType<Camera>();
        if (targetCamera == null)
            targetCamera = Object.FindAnyObjectByType<Camera>();
#else
        targetCamera = Object.FindObjectOfType<Camera>();
#endif
        return targetCamera != null;
    }
}
