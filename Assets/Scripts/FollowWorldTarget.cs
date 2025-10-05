// Assets/Scripts/BCI/FollowWorldTarget.cs
using UnityEngine;

public class FollowWorldTarget : MonoBehaviour
{
    public Transform target;
    public Vector3 worldOffset = Vector3.up * 1.6f;
    void LateUpdate()
    {
        if (!target) { enabled = false; return; }
        transform.position = target.position + worldOffset;
    }
}
