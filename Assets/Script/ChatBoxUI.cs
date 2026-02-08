using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ChatBoxUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private RectTransform messagesArea;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text placeholderText;

    [Header("Pooling")]
    [SerializeField] private ChatObjectPool chatPool;   // <-- assign in Inspector
    [SerializeField] private int preloadCount = 15;     // optional

    [Header("Layout")]
    [SerializeField] private float messageSpacing = 8f;
    [SerializeField] private int maxMessages = 10;

    [Header("Auto Hide")]
    [SerializeField] private float idleTimeToHide = 5f;

    [Header("AI Chat Dialogues")]
    [SerializeField] private List<string> aiChatDialogues;

    private readonly List<RectTransform> messages = new List<RectTransform>();

    private float idleTimer;
    private bool chatVisible = true;

    private void Start()
    {
        if (chatPool == null)
            Debug.LogWarning("[ChatBoxUI] ChatObjectPool not assigned.");

        if (chatPool != null && preloadCount > 0)
            chatPool.Prewarm(preloadCount);

        inputField.onEndEdit.AddListener(OnSubmit);
        inputField.onValueChanged.AddListener(OnTyping);

        inputField.DeactivateInputField();
        ResetIdleTimer();
    }

    private void Update()
    {
        if (!chatVisible && inputField.isFocused)
        {
            ShowChat();
            ResetIdleTimer();
            return;
        }

        if (!chatVisible &&
            (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            ShowChat();
            return;
        }

        if (chatVisible && !inputField.isFocused)
        {
            idleTimer += Time.unscaledDeltaTime;

            if (idleTimer >= idleTimeToHide)
                HideChat();
        }
    }

    private void ShowChat()
    {
        chatPanel.transform.localScale = Vector3.one;
        chatVisible = true;

        ResetIdleTimer();
        inputField.Select();
        inputField.ActivateInputField();
        placeholderText.text = " Enter chat...";
    }

    private void HideChat()
    {
        chatPanel.transform.localScale = Vector3.zero;
        chatVisible = false;

        placeholderText.text = " Press enter to open chat";
    }

    private void ResetIdleTimer()
    {
        idleTimer = 0f;
    }

    private void OnSubmit(string text)
    {
        if (!chatVisible) return;
        if (string.IsNullOrWhiteSpace(text)) return;

        CreateMessage("You: " + text, Color.cyan);
        AIChat();

        inputField.text = "";
        inputField.ActivateInputField();
        ResetIdleTimer();
    }

    private void OnTyping(string _)
    {
        if (!chatVisible) return;
        ResetIdleTimer();
    }

    private RectTransform GetMessageFromPool()
    {
        if (chatPool == null) return null;

        GameObject go = chatPool.Get();
        if (!go) return null;

        // IMPORTANT: parent it into the scroll/messages area
        go.transform.SetParent(messagesArea, false);

        var rt = go.GetComponent<RectTransform>();
        if (!rt) rt = go.AddComponent<RectTransform>();

        return rt;
    }

    private void ReturnMessageToPool(RectTransform msg)
    {
        if (!msg) return;

        // keep hierarchy tidy
        msg.gameObject.SetActive(false);

        if (chatPool != null)
            chatPool.Release(msg.gameObject);
        else
            Destroy(msg.gameObject);
    }

    private void CreateMessage(string text, Color color)
    {
        if (messagesArea == null) return;

        // Move old messages up
        float msgHeight = 40f;
        var prefabRt = messagesArea.GetComponentInChildren<RectTransform>();
        if (prefabRt != null) msgHeight = prefabRt.sizeDelta.y; // fallback-ish

        float moveUp = msgHeight + messageSpacing;

        for (int i = 0; i < messages.Count; i++)
            messages[i].anchoredPosition += Vector2.up * moveUp;

        RectTransform rt = GetMessageFromPool();
        if (!rt) return;

        rt.anchoredPosition = new Vector2(0, -messagesArea.rect.height * 0.5f);

        TMP_Text msgText = rt.GetComponent<TMP_Text>();
        if (msgText == null) msgText = rt.GetComponentInChildren<TMP_Text>();
        if (msgText != null)
        {
            msgText.text = text;
            msgText.color = color;
        }

        rt.gameObject.SetActive(true);
        messages.Add(rt);

        // Cap messages
        if (messages.Count > maxMessages)
        {
            ReturnMessageToPool(messages[0]);
            messages.RemoveAt(0);
        }
    }

    private void AIChat()
    {
        if (aiChatDialogues == null || aiChatDialogues.Count == 0) return;

        int random = Random.Range(0, aiChatDialogues.Count);
        CreateMessage("Rev: " + aiChatDialogues[random], Color.yellow);
    }
}
