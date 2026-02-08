using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GunUI : MonoBehaviour
{
    [Header("Chat (Optional)")]
    [SerializeField] private TMP_InputField chatInput;

    [Header("Settings (Optional)")]
    [SerializeField] private GameObject settingsCanvas;

    [Header("Big Map")]
    [SerializeField] private GameObject bigMap;

    [Header("Reload & Ammo UI")]
    [SerializeField] private Image reloadFill;
    [SerializeField] private float reloadDuration = 1.2f;
    [SerializeField] private int maxAmmo = 12;
    [SerializeField] private float fireRate = 0.15f;
    [SerializeField] private TMP_Text ammoText;

    private int currAmmo;
    private bool isReloading;
    private float reloadTimer;
    private float nextFireTime;

    [Header("Weapon Swap UI")]
    [SerializeField] private GameObject icon1;
    [SerializeField] private GameObject icon2;

    private int index = 0;

    private void Start()
    {
        currAmmo = maxAmmo;
        UpdateAmmoUI();

        if (reloadFill != null)
            reloadFill.fillAmount = 0f;

        ApplyGun();
    }

    private void Update()
    {
        if (IsTyping()) return;

        if (IsSettingsOpen()) return;

        if (IsMapOpen()) return;

        HandleShootingAndReload();
        HandleGunSwap();
    }

    private bool IsTyping()
    {
        return chatInput != null && chatInput.isFocused;
    }

    private bool IsSettingsOpen()
    {
        return settingsCanvas != null && settingsCanvas.activeInHierarchy;
    }

    private bool IsMapOpen()
    {
        return bigMap != null && bigMap.activeInHierarchy;
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

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("GunShoot", 0.1f);

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

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("Reload");

        if (reloadFill != null)
            reloadFill.fillAmount = 0f;
    }

    private void UpdateReload()
    {
        reloadTimer += Time.unscaledDeltaTime;

        if (reloadFill != null)
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

    private void HandleGunSwap()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (scroll == 0f) return;

        index = 1 - index;
        ApplyGun();
    }

    private void ApplyGun()
    {
        if (icon1) icon1.SetActive(index == 0);
        if (icon2) icon2.SetActive(index == 1);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("GunSwap");
    }

    public void ForceReload()
    {
        if (!isReloading) StartReload();
    }

    public int GetCurrentAmmo() => currAmmo;
    public bool IsReloading() => isReloading;
}
