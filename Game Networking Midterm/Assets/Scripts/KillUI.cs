using UnityEngine;
using Unity.Netcode;
using TMPro;

public class KillUI : MonoBehaviour
{
    [SerializeField] private TMP_Text killText;

    private PlayerHealth playerHealth;

    private void Start()
    {
        if (killText == null)
            killText = GetComponent<TMP_Text>();

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
            if (!player.IsOwner)
                continue;

            PlayerHealth health = player.GetComponent<PlayerHealth>();

            if (health == null || health == playerHealth)
                return;

            if (playerHealth != null)
                playerHealth.KillCount.OnValueChanged -= OnKillsChanged;

            playerHealth = health;
            playerHealth.KillCount.OnValueChanged += OnKillsChanged;

            UpdateKillText(playerHealth.KillCount.Value);
            return;
        }
    }

    private void OnKillsChanged(int oldValue, int newValue)
    {
        UpdateKillText(newValue);
    }

    private void UpdateKillText(int kills)
    {
        if (killText != null)
            killText.text = "Kills: " + kills;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;

        if (playerHealth != null)
            playerHealth.KillCount.OnValueChanged -= OnKillsChanged;
    }
}