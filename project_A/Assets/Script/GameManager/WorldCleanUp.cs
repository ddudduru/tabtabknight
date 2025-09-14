// WorldCleanup.cs
using UnityEngine;

public static class WorldCleanup
{
    public static void CleanupForHome()
    {
        // Stop & clear map parts
        if (MapController.Instance != null)
        {
            MapController.Instance.FullReset(clearPatterns: false);
            MapController.Instance.PurgeStraySceneObjects();
        }

        // Despawn scene-wide runtime objects
        DespawnAllEnemies();
        DespawnAllItems();
        DespawnAllObstacles();
        DespawnAllProjectiles();
    }

    public static void CleanupForRestart()
    {
        // Keep patterns; StageState.ApplyStageMap will rebuild.
        if (MapController.Instance != null)
        {
            MapController.Instance.PurgeStraySceneObjects();
        }

        DespawnAllEnemies();
        DespawnAllItems();
        DespawnAllObstacles();
        DespawnAllProjectiles();
    }

    // --------------------------
    // Scene sweep helpers
    // --------------------------

    private static T[] FindAll<T>() where T : Object
    {
#if UNITY_2023_1_OR_NEWER
        return Object.FindObjectsByType<T>(FindObjectsSortMode.None);
#else
        return Object.FindObjectsOfType<T>(true); // includeInactive = true (Unity 2020.1+)
#endif
    }
    // --------------------------
    // Utilities
    // --------------------------
    private static void SafeDespawn(GameObject go)
    {
        if (go == null)
        {
            return;
        }

        // Prefer strongly-typed Despawn on known components
        if (go.TryGetComponent<Enemy>(out var e))
        {
            e.Despawn();
            return;
        }
        if (go.TryGetComponent<Item>(out var it))
        {
            it.Despawn();
            return;
        }
        if (go.TryGetComponent<Obstacls_Control>(out var ob))
        {
            ob.Despawn();
            return;
        }
        if (go.TryGetComponent<WormProjectile>(out var pr))
        {
            pr.Despawn();
            return;
        }

        // Generic message-based fallback (covers cases where components only expose ReturnToPool via event)
        if (!TrySend(go, "Despawn"))
        {
            if (!TrySend(go, "ReturnToPool"))
            {
                Object.Destroy(go);
            }
        }
    }

    private static bool TrySend(GameObject go, string method)
    {
        try
        {
            go.SendMessage(method, SendMessageOptions.DontRequireReceiver);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void DespawnAllEnemies()
    {
        foreach (var e in FindAll<Enemy>())
        {
            if (e != null)
            {
                SafeDespawn(e.gameObject);
            }
        }
    }

    private static void DespawnAllItems()
    {
        foreach (var it in FindAll<Item>())
        {
            if (it != null)
            {
                SafeDespawn(it.gameObject);
            }
        }
    }

    private static void DespawnAllObstacles()
    {
        foreach (var ob in FindAll<Obstacls_Control>())
        {
            if (ob != null)
            {
                SafeDespawn(ob.gameObject);
            }
        }
    }

    private static void DespawnAllProjectiles()
    {
        // If you have a Projectile component, prefer that.
        foreach (var pr in FindAll<WormProjectile>())
        {
            if (pr != null)
            {
                SafeDespawn(pr.gameObject);
            }
        }
    }

    private static void PurgeProjectilesAndVFX()
    {
        // Fallback if there is no manager API or some loose runtime objects exist.
        // Add your tags if defined (e.g., ConstData.ProjectileTag, ConstData.VFXTag)
        TryDestroyByTagSafe("Projectile");
        TryDestroyByTagSafe("VFX");
    }

    private static void TryDestroyByTagSafe(string tag)
    {
        try
        {
            var arr = GameObject.FindGameObjectsWithTag(tag);
            for (int i = 0; i < arr.Length; i++)
            {
                Object.Destroy(arr[i]);
            }
        }
        catch
        {
            // Tag may not exist; ignore
        }
    }
}

