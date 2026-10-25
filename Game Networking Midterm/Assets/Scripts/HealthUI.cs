using UnityEngine;
using Unity.Netcode;
using TMPro;
    
public class HealthUI : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;

    private PlayerHealth playerHealth;

    private void Start()
    {
        if (healthText == null)
            healthText = GetComponent<TMP_Text>();

        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        FindLocalPlayer();
    }

    private void OnClientConnected(ulong clientId)
    {
        FindLocalPlayer();
    }

    private void FindLocalPlayer()
    {
        if (NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsClient)
            return;

        foreach (var player in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
        {
            if (player.IsOwner)
            {
                PlayerHealth health = player.GetComponent<PlayerHealth>();

                if (health != null && playerHealth != health)
                {
                    if (playerHealth != null)
                        playerHealth.CurrentHealth.OnValueChanged -= OnHealthChanged;

                    playerHealth = health;
                    playerHealth.CurrentHealth.OnValueChanged += OnHealthChanged;
                    UpdateHealthText(playerHealth.CurrentHealth.Value);
                }

                return;
            }
        }
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        UpdateHealthText(newValue);
    }

    private void UpdateHealthText(int health)
    {
        if (healthText != null)
            healthText.text = health + " HP";
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;

        if (playerHealth != null)
            playerHealth.CurrentHealth.OnValueChanged -= OnHealthChanged;
    }
}