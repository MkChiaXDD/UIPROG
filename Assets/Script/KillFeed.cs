using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KillFeed : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform feedContainer;

    [Header("Pooling")]
    [SerializeField] private KillfeedObjectPool killFeedPool;
    [SerializeField] private int preloadCount = 10;

    [Header("Test")]
    [SerializeField] private KeyCode spawnKey = KeyCode.K;

    [Header("Behavior")]
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private int maxActive = 6;
    [SerializeField] private bool newestOnTop = true;

    [Header("Animate")]
    [SerializeField] private float floatUpPixels = 40f;

    [SerializeField] private TMP_Text killText;
    public int playerLeft;

    private readonly LinkedList<GameObject> active = new LinkedList<GameObject>();
    private readonly Dictionary<GameObject, Coroutine> running = new Dictionary<GameObject, Coroutine>();
    private readonly Dictionary<GameObject, Vector2> startPos = new Dictionary<GameObject, Vector2>();

    private void Awake()
    {
        if (killText) killText.text = playerLeft.ToString();

        if (killFeedPool != null)
            killFeedPool.Prewarm(preloadCount);
        else
            Debug.LogWarning("[KillFeed] killFeedPool is not assigned.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(spawnKey))
        {
            PeopleDie();
            Spawn();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX("Killfeed", 0.2f);
        }
    }

    public void Spawn()
    {
        if (!feedContainer) return;
        if (!killFeedPool) return;

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

        GameObject go = killFeedPool.Get();
        if (!go) return;

        go.transform.SetParent(feedContainer, false);
        if (newestOnTop) go.transform.SetSiblingIndex(0);

        // Ensure CanvasGroup exists (your old CreateNew did this)
        var cg = go.GetComponent<CanvasGroup>();
        if (!cg) cg = go.AddComponent<CanvasGroup>();
        cg.alpha = 1f;

        // Cache starting position per instance
        var rt = go.GetComponent<RectTransform>();
        if (rt != null)
            startPos[go] = rt.anchoredPosition;

        go.SetActive(true);
        active.AddFirst(go);

        if (lifeTime > 0f)
        {
            if (running.TryGetValue(go, out var oldCo) && oldCo != null)
                StopCoroutine(oldCo);

            running[go] = StartCoroutine(FloatFadeThenRelease(go, lifeTime));
        }
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
            if (!go.activeSelf) yield break;

            float u = t / seconds;
            rt.anchoredPosition = Vector2.Lerp(from, to, u);
            cg.alpha = Mathf.Lerp(1f, 0f, u);

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        rt.anchoredPosition = to;
        cg.alpha = 0f;

        // remove from active list
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

        if (running.TryGetValue(go, out var co) && co != null)
        {
            StopCoroutine(co);
            running[go] = null;
        }

        var rt = go.GetComponent<RectTransform>();
        if (rt && startPos.TryGetValue(go, out var p))
            rt.anchoredPosition = p;

        var cg = go.GetComponent<CanvasGroup>();
        if (cg) cg.alpha = 1f;

        killFeedPool.Release(go);
    }

    private void PeopleDie()
    {
        playerLeft--;
        if (killText) killText.text = playerLeft.ToString();
    }
}
