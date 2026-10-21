using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Componente que registra las colisiones de la máquina.
/// </summary>
public class GoldbergCollisionTracker : MonoBehaviour
{
    private int collisionCount = 0;
    private readonly HashSet<string> registeredCollisions = new HashSet<string>();
    private readonly Dictionary<GameObject, string> itemLabels = new Dictionary<GameObject, string>();

    public void Register(GameObject obj, string label)
    {
        if (obj != null && !itemLabels.ContainsKey(obj))
        {
            itemLabels[obj] = label;
            var proxy = obj.AddComponent<GoldbergCollisionProxy>();
            proxy.tracker = this;
        }
    }

    public void OnCustomCollision(GameObject a, GameObject b)
    {
        if (a == null || b == null) return;

        int hashA = a.GetHashCode();
        int hashB = b.GetHashCode();
        string pairKey = hashA < hashB ? $"{hashA}_{hashB}" : $"{hashB}_{hashA}";

        if (registeredCollisions.Contains(pairKey))
        {
            return;
        }

        registeredCollisions.Add(pairKey);
        collisionCount++;

        string nameA = itemLabels.ContainsKey(a) ? itemLabels[a] : a.name;
        string nameB = itemLabels.ContainsKey(b) ? itemLabels[b] : b.name;

        Debug.Log($"💥 [Colisión #{collisionCount}] {nameA} impactó a {nameB}!");
    }
}
