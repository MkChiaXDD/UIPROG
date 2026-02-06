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
    private bool mapActive;

    [Header("Reload & Ammo")]
    [SerializeField] private Image reloadFill;
    [SerializeField] private float reloadDuration;
    [SerializeField] private int maxAmmo;
    [SerializeField] private float fireRate;
    [SerializeField] private TMP_Text ammoText;

    private int currAmmo;
    private bool isReloading;
    private float reloadTimer;
    private float nextFireTime;

    [Header("Health")]
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

    [Header("Stamina")]
    [SerializeField] private Image staminaFill;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 30f;
    [SerializeField] private float staminaRecoverRate = 25f;
    [SerializeField] private float staminaRecoverDelay = 1f;

    private float currStamina;
    private float staminaRecoverTimer;

    [Header("Weapon Swap")]
    [SerializeField] private GameObject icon1;
    [SerializeField] private GameObject icon2;

    private int index = 0;

    private void Start()
    {
        mapActive = false;
        bigMap.SetActive(false);

        currHealth = maxHealth;
        targetHealthFill = 1f;
        healthFill.fillAmount = 1f;
        healthBar.text = $"{currHealth} / {maxHealth}";

        if (volume.profile.TryGet(out vignette))
            vignette.intensity.value = 0f;

        if (lowHealthOverlay != null)
        {
            Color c = lowHealthOverlay.color;
            c.a = 0f;
            lowHealthOverlay.color = c;
        }

        currStamina = maxStamina;
        if (staminaFill != null)
            staminaFill.fillAmount = 1f;

        currAmmo = maxAmmo;
        UpdateAmmoUI();

        if (reloadFill != null)
            reloadFill.fillAmount = 0f;

        ApplyGun();
    }

    // ================= UPDATE =================
    private void Update()
    {
        if (IsTyping()) return;

        HandleInput();
        UpdateHealthUI();
        UpdatePostProcessing();
        HandleStamina();
        HandleShootingAndReload();
        HandleGunSwap();
    }

    private bool IsTyping()
    {
        return chatInput != null && chatInput.isFocused;
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.M))
            ToggleMap();

        if (Input.GetKeyDown(KeyCode.O))
            Damage();

        if (Input.GetKeyDown(KeyCode.P))
            Heal();
    }

    public void ToggleMap()
    {
        mapActive = !mapActive;
        bigMap.SetActive(mapActive);
    }

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
        targetHealthFill = (float)currHealth / maxHealth;
        healthBar.text = $"{currHealth} / {maxHealth}";
    }

    private void UpdateHealthUI()
    {
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
            float t = hpPercent / lowHealthThreshold;
            targetOverlayAlpha = Mathf.Lerp(maxOverlayAlpha, 0f, t);
        }
        else
        {
            targetOverlayAlpha = 0f;
        }
    }

    private void HandleStamina()
    {
        bool sprinting = Input.GetKey(KeyCode.LeftShift);

        if (sprinting && currStamina > 0f)
        {
            staminaRecoverTimer = staminaRecoverDelay;
            currStamina -= staminaDrainRate * Time.unscaledDeltaTime;
        }
        else
        {
            if (staminaRecoverTimer > 0f)
                staminaRecoverTimer -= Time.unscaledDeltaTime;
            else
                currStamina += staminaRecoverRate * Time.unscaledDeltaTime;
        }

        currStamina = Mathf.Clamp(currStamina, 0f, maxStamina);

        if (staminaFill != null)
            staminaFill.fillAmount = currStamina / maxStamina;
    }

    private void HandleShootingAndReload()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currAmmo < maxAmmo)
            StartReload();

        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
            TryShoot();

        if (isReloading)
            UpdateReload();
    }

    private void TryShoot()
    {
        if (isReloading || currAmmo <= 0) return;

        currAmmo--;
        nextFireTime = Time.time + fireRate;
        UpdateAmmoUI();

        if (currAmmo <= 0)
            StartReload();
    }

    private void StartReload()
    {
        isReloading = true;
        reloadTimer = 0f;

        if (reloadFill != null)
            reloadFill.fillAmount = 0f;
    }

    private void UpdateReload()
    {
        reloadTimer += Time.unscaledDeltaTime;
        reloadFill.fillAmount = reloadTimer / reloadDuration;

        if (reloadTimer >= reloadDuration)
            FinishReload();
    }

    private void FinishReload()
    {
        isReloading = false;
        currAmmo = maxAmmo;
        reloadTimer = 0f;

        UpdateAmmoUI();

        if (reloadFill != null)
            reloadFill.fillAmount = 0f;
    }

    private void UpdateAmmoUI()
    {
        if (ammoText != null)
            ammoText.text = $"{currAmmo} / {maxAmmo}";
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void HandleGunSwap()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (scroll == 0f) return;

        index = 1 - index;   // swap
        ApplyGun();
    }

    private void ApplyGun()
    {
        if (icon1) icon1.SetActive(index == 0);
        if (icon2) icon2.SetActive(index == 1);
    }
}
