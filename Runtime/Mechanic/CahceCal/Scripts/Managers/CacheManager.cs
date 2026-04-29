using System.Collections.Generic;
using UnityEngine;

public class CacheManager : MonoBehaviour
{
    [SerializeField] private Card[] cacheSlots; // CacheSlot1-3

    private List<int> cache = new List<int>();
    private int maxSize = 2;

    void Start()
    {
        RefreshUI();
    }

    public int MaxSize => maxSize;

    public void SetMaxSize(int size)
    {
        maxSize = size;
    }

    // FIFO: evict oldest when full
    public void AddValue(int value)
    {
        if (cache.Count >= maxSize)
        {
            Debug.Log($"[Cache] FIFO evict: {cache[0]}");
            cache.RemoveAt(0);
        }
        cache.Add(value);
        Debug.Log($"[Cache] Added {value} | cache=[{string.Join(",", cache)}]");
        RefreshUI();
    }

    public List<int> GetValues()
    {
        return new List<int>(cache);
    }

    public bool Contains(int value)
    {
        return cache.Contains(value);
    }

    public void Clear()
    {
        cache.Clear();
        RefreshUI();
    }

    private void RefreshUI()
    {
        for (int i = 0; i < cacheSlots.Length; i++)
        {
            if (i < cache.Count)
            {
                cacheSlots[i].SetValue(cache[i]);
                cacheSlots[i].ResetCard();
            }
            else
            {
                cacheSlots[i].ClearSlot();
            }
        }
    }
}

