using TMPro;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIElements : MonoBehaviour
{
    [SerializeField] private TMP_InputField chatInput;
    [SerializeField] private GameObject bigMap;
    private bool mapActive = false;

    [SerializeField] private Image healthFill;
    [SerializeField] private int maxHealth;
    private int currHealth;

    private void Start()
    {
        mapActive = false;
        bigMap.SetActive(mapActive);

        currHealth = maxHealth;
    }
    private void Update()
    {
        if (chatInput != null && chatInput.isFocused)
            return;

        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMap();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            Damage();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Heal();
        }
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
    }

    private void Heal()
    {
        if (currHealth >= maxHealth) return;
        currHealth++;
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        healthFill.fillAmount = (float)currHealth / maxHealth;
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
