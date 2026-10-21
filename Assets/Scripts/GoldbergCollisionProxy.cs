using UnityEngine;

/// <summary>
/// Reenvía los eventos OnCollisionEnter al tracker principal.
/// </summary>
public class GoldbergCollisionProxy : MonoBehaviour
{
    [HideInInspector] public GoldbergCollisionTracker tracker;

    private void OnCollisionEnter(Collision collision)
    {
        if (tracker != null && collision.gameObject != null)
        {
            tracker.OnCustomCollision(gameObject, collision.gameObject);
        }
    }
}
