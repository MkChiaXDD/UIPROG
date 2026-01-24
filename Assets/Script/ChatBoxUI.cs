using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ChatBoxUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private RectTransform messagesArea;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject messagePrefab;

    [Header("Layout")]
    [SerializeField] private float messageSpacing = 8f;
    [SerializeField] private int maxMessages = 10;

    [Header("Auto Hide")]
    [SerializeField] private float idleTimeToHide = 5f;

    [Header("Ai Chat Dialogues")]
    [SerializeField] List<string> aiChatDialogues;

    private List<RectTransform> messages = new List<RectTransform>();
    private float idleTimer;
    private bool chatOpen = true;

    void Start()
    {
        inputField.onEndEdit.AddListener(OnSubmit);
        ResetIdleTimer();
    }

    void Update()
    {
        // ENTER when chat is CLOSED ? open chat
        if (!chatOpen &&
            (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            OpenChat();
            return;
        }

        // Idle countdown when chat is open
        if (chatOpen)
        {
            idleTimer += Time.unscaledDeltaTime;

            if (idleTimer >= idleTimeToHide)
            {
                CloseChat();
            }
        }
    }

    // =====================
    // CHAT OPEN / CLOSE
    // =====================
    private void OpenChat()
    {
        chatPanel.SetActive(true);
        chatOpen = true;
        ResetIdleTimer();

        inputField.ActivateInputField();
    }

    private void CloseChat()
    {
        chatPanel.SetActive(false);
        chatOpen = false;
    }

    private void ResetIdleTimer()
    {
        idleTimer = 0f;
    }

    // =====================
    // MESSAGE SUBMIT
    // =====================
    private void OnSubmit(string text)
    {
        // Only send if chat is open
        if (!chatOpen)
            return;

        // ENTER detection (InputField eats Enter)
        if (!Input.GetKeyDown(KeyCode.Return) &&
            !Input.GetKeyDown(KeyCode.KeypadEnter))
            return;

        if (string.IsNullOrWhiteSpace(text))
            return;

        string finalText = "You: " + text;

        CreateMessage(finalText, Color.softBlue);
        AIChat();

        inputField.text = "";
        inputField.ActivateInputField();
        ResetIdleTimer();
    }

    // =====================
    // CREATE MESSAGE
    // =====================
    private void CreateMessage(string text, Color color)
    {
        float moveUp =
            messagePrefab.GetComponent<RectTransform>().sizeDelta.y
            + messageSpacing;

        // Push old messages up
        foreach (RectTransform msg in messages)
        {
            msg.anchoredPosition += Vector2.up * moveUp;
        }

        // Spawn new message at bottom
        GameObject newMsg = Instantiate(messagePrefab, messagesArea);
        RectTransform rt = newMsg.GetComponent<RectTransform>();
        rt.anchoredPosition = rt.anchoredPosition = new Vector2(0, -messagesArea.rect.height * 0.5f);

        newMsg.GetComponent<TMP_Text>().text = text;
        newMsg.GetComponent<TMP_Text>().color = color;

        messages.Add(rt);

        // Remove oldest
        if (messages.Count > maxMessages)
        {
            Destroy(messages[0].gameObject);
            messages.RemoveAt(0);
        }
    }

    private void AIChat()
    {
        int random = Random.Range(0, aiChatDialogues.Count);

        string newText = "Rev: " + aiChatDialogues[random];

        CreateMessage(newText, Color.orange);
    }
}
