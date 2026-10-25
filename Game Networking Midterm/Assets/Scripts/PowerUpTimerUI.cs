using UnityEngine;
using TMPro;
using Unity.Netcode;

public class PowerUpTimerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text damageBoostText;
    [SerializeField] private TMP_Text invincibilityText;

    private PlayerHealth localPlayer;

    private void Update()
    {
        // Find the local player's health component.
        if (localPlayer == null || !localPlayer.IsSpawned)
        {
            localPlayer = null;

            foreach (NetworkObject player in
                FindObjectsByType<NetworkObject>(
                    FindObjectsSortMode.None))
            {
                if (player.IsSpawned && player.IsOwner)
                {
                    localPlayer = player.GetComponent<PlayerHealth>();

                    if (localPlayer != null)
                        break;
                }
            }
        }

        if (localPlayer == null)
        {
            SetTexts(
                "Damage Boost: Inactive",
                "Invincibility: Inactive"
            );
            return;
        }

        float damageTime =
            localPlayer.DamageBoostTimeRemaining.Value;

        float invincibilityTime =
            localPlayer.InvincibilityTimeRemaining.Value;

        string damageLabel = damageTime > 0f
            ? $"Damage Boost: {Mathf.CeilToInt(damageTime)}s"
            : "Damage Boost: Inactive";

        string invincibilityLabel = invincibilityTime > 0f
            ? $"Invincibility: {Mathf.CeilToInt(invincibilityTime)}s"
            : "Invincibility: Inactive";

        SetTexts(damageLabel, invincibilityLabel);
    }

    private void SetTexts(string damage, string invincibility)
    {
        if (damageBoostText != null)
            damageBoostText.text = damage;

        if (invincibilityText != null)
            invincibilityText.text = invincibility;
    }
}