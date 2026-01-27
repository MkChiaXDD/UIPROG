using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ChatBoxUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject chatPanel;          // stays ACTIVE
    [SerializeField] private RectTransform messagesArea;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text placeholderText;
    [SerializeField] private GameObject messagePrefab;

    [Header("Layout")]
    [SerializeField] private float messageSpacing = 8f;
    [SerializeField] private int maxMessages = 10;

    [Header("Auto Hide")]
    [SerializeField] private float idleTimeToHide = 5f;

    [Header("AI Chat Dialogues")]
    [SerializeField] private List<string> aiChatDialogues;

    private List<RectTransform> messages = new List<RectTransform>();
    private float idleTimer;
    private bool chatVisible = true;

    void Start()
    {
        inputField.onEndEdit.AddListener(OnSubmit);
        inputField.onValueChanged.AddListener(OnTyping);

        ShowChat();
        inputField.ActivateInputField();
        ResetIdleTimer();
    }

    void Update()
    {
        // Click input field → open chat
        if (!chatVisible && inputField.isFocused)
        {
            ShowChat();
            ResetIdleTimer();
            return;
        }

        // ENTER when chat is hidden → show it
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
            {
                HideChat();
            }
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
        chatPanel.transform.localScale = Vector3.zero; // invisible but ACTIVE
        chatVisible = false;

        placeholderText.text = " Press enter to open chat";
    }

    private void ResetIdleTimer()
    {
        idleTimer = 0f;
    }

    private void OnSubmit(string text)
    {
        if (!chatVisible)
            return;

        if (string.IsNullOrWhiteSpace(text))
            return;

        CreateMessage("You: " + text, Color.cyan);
        AIChat();

        inputField.text = "";
        inputField.ActivateInputField();
        ResetIdleTimer();
    }


    private void CreateMessage(string text, Color color)
    {
        float moveUp =
            messagePrefab.GetComponent<RectTransform>().sizeDelta.y
            + messageSpacing;

        foreach (RectTransform msg in messages)
        {
            msg.anchoredPosition += Vector2.up * moveUp;
        }

        GameObject newMsg = Instantiate(messagePrefab, messagesArea);
        RectTransform rt = newMsg.GetComponent<RectTransform>();

        rt.anchoredPosition = new Vector2(0, -messagesArea.rect.height * 0.5f);

        TMP_Text msgText = newMsg.GetComponent<TMP_Text>();
        msgText.text = text;
        msgText.color = color;

        messages.Add(rt);

        if (messages.Count > maxMessages)
        {
            Destroy(messages[0].gameObject);
            messages.RemoveAt(0);
        }
    }

    private void AIChat()
    {
        if (aiChatDialogues == null || aiChatDialogues.Count == 0)
            return;

        int random = Random.Range(0, aiChatDialogues.Count);
        CreateMessage("Rev: " + aiChatDialogues[random], Color.yellow);
    }

    private void OnTyping(string _)
    {
        if (!chatVisible)
            return;

        ResetIdleTimer();
    }
}
