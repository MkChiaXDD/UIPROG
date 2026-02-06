using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KillFeed : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform feedContainer;
    [SerializeField] private GameObject feedPrefab;

    [Header("Pooling")]
    [SerializeField] private int preloadCount = 10;

    [Header("Test")]
    [SerializeField] private KeyCode spawnKey = KeyCode.K;

    [Header("Behavior")]
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private int maxActive = 6;         // 0 = unlimited
    [SerializeField] private bool newestOnTop = true;

    [Header("Animate")]
    [SerializeField] private float floatUpPixels = 40f; // how far it floats up before disappearing

    [SerializeField] private TMP_Text killText;
    public int playerLeft;

    private readonly Queue<GameObject> pool = new Queue<GameObject>();
    private readonly LinkedList<GameObject> active = new LinkedList<GameObject>();

    private readonly Dictionary<GameObject, Coroutine> running = new Dictionary<GameObject, Coroutine>();
    private readonly Dictionary<GameObject, Vector2> startPos = new Dictionary<GameObject, Vector2>();

    private void Awake()
    {
        Preload();
        killText.text = playerLeft.ToString();
    }

    private void Update()
    {
        if (Input.GetKeyDown(spawnKey))
        {
            PeopleDie();
            Spawn();
        }
    }

    private void Preload()
    {
        if (!feedPrefab) return;

        for (int i = 0; i < preloadCount; i++)
        {
            var go = CreateNew();
            Release(go);
        }
    }

    private GameObject CreateNew()
    {
        var go = Instantiate(feedPrefab);
        go.SetActive(false);

        // make sure it can fade
        if (!go.GetComponent<CanvasGroup>())
            go.AddComponent<CanvasGroup>();

        return go;
    }

    public void Spawn()
    {
        if (!feedContainer || !feedPrefab) return;

        // cap active items (remove oldest)
        if (maxActive > 0)
        {
            while (active.Count >= maxActive)
            {
                var oldestNode = active.Last;
                if (oldestNode == null) break;

                var oldest = oldestNode.Value;
                active.RemoveLast();
                Release(oldest);
            }
        }

        GameObject go = Get();

        go.transform.SetParent(feedContainer, false);
        if (newestOnTop) go.transform.SetSiblingIndex(0);

        // reset visuals before show
        var cg = go.GetComponent<CanvasGroup>();
        cg.alpha = 1f;

        var rt = go.GetComponent<RectTransform>();
        startPos[go] = rt.anchoredPosition; // remember where layout placed it

        go.SetActive(true);
        active.AddFirst(go);

        // start animation coroutine
        if (lifeTime > 0f)
        {
            if (running.TryGetValue(go, out var oldCo) && oldCo != null)
                StopCoroutine(oldCo);

            running[go] = StartCoroutine(FloatFadeThenRelease(go, lifeTime));
        }
    }

    private GameObject Get()
    {
        if (pool.Count > 0)
            return pool.Dequeue();

        return CreateNew();
    }

    private IEnumerator FloatFadeThenRelease(GameObject go, float seconds)
    {
        if (!go) yield break;

        var rt = go.GetComponent<RectTransform>();
        var cg = go.GetComponent<CanvasGroup>();

        if (!rt || !cg) yield break;

        Vector2 from = startPos.TryGetValue(go, out var p) ? p : rt.anchoredPosition;
        Vector2 to = from + new Vector2(0f, floatUpPixels);

        float t = 0f;
        while (t < seconds)
        {
            // if it got released early (cap), stop
            if (!go.activeSelf) yield break;

            float u = t / seconds;

            // move up + fade out
            rt.anchoredPosition = Vector2.Lerp(from, to, u);
            cg.alpha = Mathf.Lerp(1f, 0f, u);

            t += Time.unscaledDeltaTime; // UI usually feels better unscaled
            yield return null;
        }

        // ensure end state then release
        rt.anchoredPosition = to;
        cg.alpha = 0f;

        // remove from active list safely
        var node = active.First;
        while (node != null)
        {
            if (node.Value == go)
            {
                active.Remove(node);
                break;
            }
            node = node.Next;
        }

        Release(go);
    }

    private void Release(GameObject go)
    {
        if (!go) return;

        // stop any running animation
        if (running.TryGetValue(go, out var co) && co != null)
        {
            StopCoroutine(co);
            running[go] = null;
        }

        // reset transform + alpha so next spawn starts clean
        var rt = go.GetComponent<RectTransform>();
        if (rt && startPos.TryGetValue(go, out var p))
            rt.anchoredPosition = p;

        var cg = go.GetComponent<CanvasGroup>();
        if (cg) cg.alpha = 1f;

        go.SetActive(false);

        // keep hierarchy clean (optional)
        go.transform.SetParent(transform, false);

        pool.Enqueue(go);
        
    }

    private void PeopleDie()
    {
        playerLeft--;
        killText.text = playerLeft.ToString();
    }
}
