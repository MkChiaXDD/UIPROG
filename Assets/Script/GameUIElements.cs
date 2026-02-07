using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIElements : MonoBehaviour
{
    [Header("Chat")]
    [SerializeField] private TMP_InputField chatInput;

    [Header("Map")]
    [SerializeField] private GameObject bigMap;
    private bool mapActive;

    [Header("Stamina")]
    [SerializeField] private Image staminaFill;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 30f;
    [SerializeField] private float staminaRecoverRate = 25f;
    [SerializeField] private float staminaRecoverDelay = 1f;

    private float currStamina;
    private float staminaRecoverTimer;

    [Header("Sprint Audio")]
    [SerializeField] private float sprintAudioInterval = 0.5f;

    private float sprintAudioTimer;

    private void Start()
    {
        mapActive = false;
        if (bigMap) bigMap.SetActive(false);

        currStamina = maxStamina;
        if (staminaFill != null)
            staminaFill.fillAmount = 1f;

        sprintAudioTimer = 0f;
    }

    private void Update()
    {
        if (IsTyping()) return;

        HandleInput();
        HandleStamina();
    }

    private bool IsTyping()
    {
        return chatInput != null && chatInput.isFocused;
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.M))
            ToggleMap();
    }

    public void ToggleMap()
    {
        mapActive = !mapActive;
        if (bigMap) bigMap.SetActive(mapActive);
    }

    private void HandleStamina()
    {
        bool sprinting = Input.GetKey(KeyCode.LeftShift);
        bool canSprint = currStamina > 0f;

        if (sprinting && canSprint)
        {
            staminaRecoverTimer = staminaRecoverDelay;
            currStamina -= staminaDrainRate * Time.unscaledDeltaTime;

            HandleSprintAudio(true);
        }
        else
        {
            HandleSprintAudio(false);

            if (staminaRecoverTimer > 0f)
                staminaRecoverTimer -= Time.unscaledDeltaTime;
            else
                currStamina += staminaRecoverRate * Time.unscaledDeltaTime;
        }

        currStamina = Mathf.Clamp(currStamina, 0f, maxStamina);

        if (staminaFill != null)
            staminaFill.fillAmount = currStamina / maxStamina;
    }

    private void HandleSprintAudio(bool isSprinting)
    {
        if (!isSprinting)
        {
            sprintAudioTimer = 0f;
            return;
        }

        sprintAudioTimer += Time.unscaledDeltaTime;

        if (sprintAudioTimer >= sprintAudioInterval)
        {
            sprintAudioTimer = 0f;

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX("Sprint");
        }
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
