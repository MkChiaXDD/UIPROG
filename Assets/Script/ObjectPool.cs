using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] protected GameObject prefab;
    [SerializeField] protected int initialSize = 10;

    protected readonly Queue<GameObject> pool = new Queue<GameObject>();

    protected virtual void Awake()
    {
        Prewarm(initialSize);
    }

    public virtual void Prewarm(int count)
    {
        if (!prefab) return;

        for (int i = 0; i < count; i++)
        {
            var obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public virtual GameObject Get()
    {
        if (!prefab) return null;

        var obj = pool.Count > 0 ? pool.Dequeue() : Instantiate(prefab, transform);
        obj.SetActive(true);
        return obj;
    }

    public virtual void Release(GameObject obj)
    {
        if (!obj) return;

        obj.SetActive(false);
        obj.transform.SetParent(transform, false);
        pool.Enqueue(obj);
    }
}
