using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class HealthUIController : MonoBehaviour
{
    [Header("Chat (Optional)")]
    [SerializeField] private TMP_InputField chatInput;

    [Header("Health UI")]
    [SerializeField] private Image healthFill;
    [SerializeField] private TMP_Text healthBar;
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float healthLerpSpeed = 5f;

    private int currHealth;
    private float targetHealthFill;

    [Header("Post Processing (Damage Flash)")]
    [SerializeField] private Volume volume;
    [SerializeField] private float damageVignetteIntensity = 0.45f;
    [SerializeField] private float vignetteFadeSpeed = 8f;

    private Vignette vignette;
    private float targetVignetteIntensity;

    [Header("Low Health Red Screen")]
    [SerializeField] private Image lowHealthOverlay;
    [SerializeField, Range(0f, 1f)] private float lowHealthThreshold = 0.3f;
    [SerializeField, Range(0f, 1f)] private float maxOverlayAlpha = 0.25f;
    [SerializeField] private float overlayFadeSpeed = 3f;

    private float targetOverlayAlpha;

    [Header("Debug Keys (Optional)")]
    [SerializeField] private bool enableTestKeys = true; // O = damage, P = heal

    private void Start()
    {
        currHealth = maxHealth;
        targetHealthFill = 1f;

        if (healthFill) healthFill.fillAmount = 1f;
        if (healthBar) healthBar.text = $"{currHealth} / {maxHealth}";

        if (volume != null && volume.profile != null && volume.profile.TryGet(out vignette))
            vignette.intensity.value = 0f;

        if (lowHealthOverlay != null)
        {
            Color c = lowHealthOverlay.color;
            c.a = 0f;
            lowHealthOverlay.color = c;
        }

        UpdateLowHealthOverlay();
    }

    private void Update()
    {
        if (IsTyping()) return;

        if (enableTestKeys)
            HandleTestKeys();

        UpdateHealthUI();
        UpdatePostProcessing();
    }

    private bool IsTyping()
    {
        return chatInput != null && chatInput.isFocused;
    }

    private void HandleTestKeys()
    {
        if (Input.GetKeyDown(KeyCode.O))
            TakeDamage(1);

        if (Input.GetKeyDown(KeyCode.P))
            Heal(1);
    }

    // -------------------------
    // Public API (call from your player / enemy scripts)
    // -------------------------
    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        if (currHealth <= 0) return;

        currHealth = Mathf.Max(0, currHealth - amount);

        AudioManager.Instance.PlaySFX("GetHit");
        UpdateHealthBar();
        TriggerDamageFlash();
        UpdateLowHealthOverlay();
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        if (currHealth >= maxHealth) return;

        currHealth = Mathf.Min(maxHealth, currHealth + amount);

        UpdateHealthBar();
        UpdateLowHealthOverlay();
    }

    public void SetMaxHealth(int newMax, bool refill = true)
    {
        maxHealth = Mathf.Max(1, newMax);

        if (refill)
            currHealth = maxHealth;
        else
            currHealth = Mathf.Clamp(currHealth, 0, maxHealth);

        UpdateHealthBar();
        UpdateLowHealthOverlay();

        if (healthFill)
            healthFill.fillAmount = (float)currHealth / maxHealth;
    }

    public int GetHealth() => currHealth;
    public int GetMaxHealth() => maxHealth;

    // -------------------------
    // Internals
    // -------------------------
    private void UpdateHealthBar()
    {
        targetHealthFill = (float)currHealth / maxHealth;
        if (healthBar) healthBar.text = $"{currHealth} / {maxHealth}";
    }

    private void UpdateHealthUI()
    {
        if (!healthFill) return;

        healthFill.fillAmount = Mathf.Lerp(
            healthFill.fillAmount,
            targetHealthFill,
            Time.unscaledDeltaTime * healthLerpSpeed
        );
    }

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

    private void UpdatePostProcessing()
    {
        if (vignette != null)
        {
            vignette.intensity.value = Mathf.Lerp(
                vignette.intensity.value,
                targetVignetteIntensity,
                Time.unscaledDeltaTime * vignetteFadeSpeed
            );
        }

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

    private void UpdateLowHealthOverlay()
    {
        float hpPercent = (float)currHealth / maxHealth;

        if (hpPercent <= lowHealthThreshold)
        {
            float t = hpPercent / lowHealthThreshold; // 0..1
            targetOverlayAlpha = Mathf.Lerp(maxOverlayAlpha, 0f, t);
        }
        else
        {
            targetOverlayAlpha = 0f;
        }
    }
}
