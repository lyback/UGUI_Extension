using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class MaterialCache
{
    public string key { get; private set; }

    public int referenceCount { get; private set; }

    public Material material { get; private set; }

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
    static void ClearCache()
    {
        foreach (var cache in materialCaches)
        {
            cache.material = null;
        }
        materialCaches.Clear();
    }
#endif

    public static List<MaterialCache> materialCaches = new List<MaterialCache>();

    public static MaterialCache Register(string key, System.Func<Material> onCreateMaterial)
    {
        var cache = materialCaches.FirstOrDefault(x => x.key == key);
        if (cache != null)
        {
            cache.referenceCount++;
        }
        if (cache == null)
        {
            cache = new MaterialCache()
            {
                key = key,
                material = onCreateMaterial(),
                referenceCount = 1,
            };
            materialCaches.Add(cache);
        }
        return cache;
    }

    public static void Unregister(MaterialCache cache)
    {
        if (cache == null)
        {
            return;
        }

        cache.referenceCount--;
        if (cache.referenceCount <= 0)
        {
            MaterialCache.materialCaches.Remove(cache);
            cache.material = null;
        }
    }
}