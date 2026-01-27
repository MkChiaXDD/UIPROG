using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIElements : MonoBehaviour
{
    [Header("Chat")]
    [SerializeField] private TMP_InputField chatInput;

    [Header("Map")]
    [SerializeField] private GameObject bigMap;
    private bool mapActive = false;

    [Header("Health")]
    [SerializeField] private Image healthFill;
    [SerializeField] private TMP_Text healthBar;
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float healthLerpSpeed = 5f;

    [Header("Post Processing (Damage Flash)")]
    [SerializeField] private Volume volume;
    [SerializeField] private float damageVignetteIntensity = 0.45f;
    [SerializeField] private float vignetteFadeSpeed = 8f;

    [Header("Low Health Red Screen")]
    [SerializeField] private Image lowHealthOverlay;
    [SerializeField, Range(0f, 1f)] private float lowHealthThreshold = 0.3f;
    [SerializeField, Range(0f, 1f)] private float maxOverlayAlpha = 0.25f;
    [SerializeField] private float overlayFadeSpeed = 3f;

    private int currHealth;
    private float targetFillAmount;

    // Post processing
    private Vignette vignette;
    private float targetVignetteIntensity = 0f;

    // Overlay
    private float targetOverlayAlpha = 0f;

    // ================= START =================

    private void Start()
    {
        mapActive = false;
        bigMap.SetActive(false);

        currHealth = maxHealth;
        targetFillAmount = 1f;
        healthFill.fillAmount = 1f;
        healthBar.text = $"{currHealth} / {maxHealth}";

        // Get vignette
        if (volume.profile.TryGet(out vignette))
            vignette.intensity.value = 0f;

        // Reset overlay
        if (lowHealthOverlay != null)
        {
            Color c = lowHealthOverlay.color;
            c.a = 0f;
            lowHealthOverlay.color = c;
        }
    }

    private void Update()
    {
        if (chatInput != null && chatInput.isFocused)
            return;

        if (Input.GetKeyDown(KeyCode.M))
            ToggleMap();

        if (Input.GetKeyDown(KeyCode.O))
            Damage();

        if (Input.GetKeyDown(KeyCode.P))
            Heal();

        // Smooth health bar
        healthFill.fillAmount = Mathf.Lerp(
            healthFill.fillAmount,
            targetFillAmount,
            Time.unscaledDeltaTime * healthLerpSpeed
        );

        // Vignette fade
        if (vignette != null)
        {
            vignette.intensity.value = Mathf.Lerp(
                vignette.intensity.value,
                targetVignetteIntensity,
                Time.unscaledDeltaTime * vignetteFadeSpeed
            );
        }

        // Overlay fade
        if (lowHealthOverlay != null)
        {
            Color c = lowHealthOverlay.color;
            c.a = Mathf.Lerp(
                c.a,
                targetOverlayAlpha,
                Time.unscaledDeltaTime * overlayFadeSpeed
            );
            lowHealthOverlay.color = c;
        }
    }

    // ================= MAP =================

    public void ToggleMap()
    {
        mapActive = !mapActive;
        bigMap.SetActive(mapActive);
    }

    // ================= HEALTH =================

    private void Damage()
    {
        if (currHealth <= 0) return;

        currHealth--;
        UpdateHealthBar();
        TriggerDamageFlash();
        UpdateLowHealthOverlay();
    }

    private void Heal()
    {
        if (currHealth >= maxHealth) return;

        currHealth++;
        UpdateHealthBar();
        UpdateLowHealthOverlay();
    }

    private void UpdateHealthBar()
    {
        targetFillAmount = (float)currHealth / maxHealth;
        healthBar.text = $"{currHealth} / {maxHealth}";
    }

    // ================= DAMAGE FLASH =================

    private void TriggerDamageFlash()
    {
        if (vignette == null) return;

        targetVignetteIntensity = damageVignetteIntensity;
        CancelInvoke(nameof(ResetVignette));
        Invoke(nameof(ResetVignette), 0.1f);
    }

    private void ResetVignette()
    {
        targetVignetteIntensity = 0f;
    }

    // ================= LOW HEALTH OVERLAY =================

    private void UpdateLowHealthOverlay()
    {
        float hpPercent = (float)currHealth / maxHealth;

        if (hpPercent <= lowHealthThreshold)
        {
            float t = hpPercent / lowHealthThreshold;
            targetOverlayAlpha = Mathf.Lerp(maxOverlayAlpha, 0f, t);
        }
        else
        {
            targetOverlayAlpha = 0f;
        }
    }

    // ================= MENU =================

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
