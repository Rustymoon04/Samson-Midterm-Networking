using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float respawnDelay = 2f;
    [SerializeField] private int defaultDamage = 25;

    public NetworkVariable<int> CurrentHealth =
        new NetworkVariable<int>(100);

    public NetworkVariable<bool> IsDead =
        new NetworkVariable<bool>(false);

    public NetworkVariable<int> KillCount =
        new NetworkVariable<int>(0);

    public NetworkVariable<bool> IsInvincible =
        new NetworkVariable<bool>(false);

    public NetworkVariable<int> CurrentDamage =
        new NetworkVariable<int>(25);

    public NetworkVariable<float> DamageBoostTimeRemaining =
        new NetworkVariable<float>(0f);

    public NetworkVariable<float> InvincibilityTimeRemaining =
        new NetworkVariable<float>(0f);

    private Coroutine damageBoostCoroutine;
    private Coroutine invincibilityCoroutine;
    private Coroutine respawnCoroutine;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            CurrentHealth.Value = maxHealth;
            CurrentDamage.Value = defaultDamage;
        }
    }

    public void TakeDamage(int damage, ulong shooterClientId)
    {
        if (!IsServer || !IsSpawned || IsDead.Value)
            return;

        if (IsInvincible.Value || damage <= 0)
            return;

        CurrentHealth.Value =
            Mathf.Max(0, CurrentHealth.Value - damage);

        if (CurrentHealth.Value <= 0)
        {
            IsDead.Value = true;

            Debug.Log($"Player {OwnerClientId} was eliminated.");

            if (shooterClientId != OwnerClientId &&
                NetworkManager.Singleton != null &&
                NetworkManager.Singleton.ConnectedClients.TryGetValue(
                    shooterClientId, out var shooterClient))
            {
                PlayerHealth shooterHealth =
                    shooterClient.PlayerObject != null
                    ? shooterClient.PlayerObject.GetComponent<PlayerHealth>()
                    : null;

                if (shooterHealth != null)
                    shooterHealth.KillCount.Value++;
            }

            if (respawnCoroutine != null)
                StopCoroutine(respawnCoroutine);

            respawnCoroutine = StartCoroutine(RespawnAfterDelay());
        }
    }

    public void ActivateDamagePowerUp(
        float duration, int boostedDamage)
    {
        if (!IsServer || !IsSpawned || IsDead.Value)
            return;

        if (damageBoostCoroutine != null)
            StopCoroutine(damageBoostCoroutine);

        damageBoostCoroutine =
            StartCoroutine(DamageBoostRoutine(duration, boostedDamage));
    }

    private IEnumerator DamageBoostRoutine(
        float duration, int boostedDamage)
    {
        CurrentDamage.Value = boostedDamage;

        float remaining = Mathf.Max(0f, duration);
        DamageBoostTimeRemaining.Value = remaining;

        while (remaining > 0f && IsSpawned && !IsDead.Value)
        {
            yield return new WaitForSeconds(0.1f);

            remaining = Mathf.Max(0f, remaining - 0.1f);
            DamageBoostTimeRemaining.Value = remaining;
        }

        CurrentDamage.Value = defaultDamage;
        DamageBoostTimeRemaining.Value = 0f;
        damageBoostCoroutine = null;
    }

    public void ActivateInvincibility(float duration)
    {
        if (!IsServer || !IsSpawned || IsDead.Value)
            return;

        if (invincibilityCoroutine != null)
            StopCoroutine(invincibilityCoroutine);

        invincibilityCoroutine =
            StartCoroutine(InvincibilityRoutine(duration));
    }

    private IEnumerator InvincibilityRoutine(float duration)
    {
        IsInvincible.Value = true;

        float remaining = Mathf.Max(0f, duration);
        InvincibilityTimeRemaining.Value = remaining;

        while (remaining > 0f && IsSpawned && !IsDead.Value)
        {
            yield return new WaitForSeconds(0.1f);

            remaining = Mathf.Max(0f, remaining - 0.1f);
            InvincibilityTimeRemaining.Value = remaining;
        }

        IsInvincible.Value = false;
        InvincibilityTimeRemaining.Value = 0f;
        invincibilityCoroutine = null;
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);

        if (!IsServer || !IsSpawned)
            yield break;

        MatchManager matchManager =
            FindFirstObjectByType<MatchManager>();

        if (matchManager == null ||
            !matchManager.IsSpawned ||
            !matchManager.IsMatchActive.Value)
        {
            respawnCoroutine = null;
            yield break;
        }

        // Stop active power-up effects before respawning.
        ResetPowerUps();

        PlayerSpawnManager spawnManager =
            FindFirstObjectByType<PlayerSpawnManager>();

        if (spawnManager != null)
        {
            spawnManager.RespawnPlayer(OwnerClientId);

            CurrentHealth.Value = maxHealth;
            IsDead.Value = false;
        }
        else
        {
            Debug.LogWarning("PlayerSpawnManager was not found.");
        }

        respawnCoroutine = null;
    }

    private void ResetPowerUps()
    {
        if (damageBoostCoroutine != null)
        {
            StopCoroutine(damageBoostCoroutine);
            damageBoostCoroutine = null;
        }

        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
            invincibilityCoroutine = null;
        }

        CurrentDamage.Value = defaultDamage;
        IsInvincible.Value = false;
        DamageBoostTimeRemaining.Value = 0f;
        InvincibilityTimeRemaining.Value = 0f;
    }

    public override void OnNetworkDespawn()
    {
        if (damageBoostCoroutine != null)
            StopCoroutine(damageBoostCoroutine);

        if (invincibilityCoroutine != null)
            StopCoroutine(invincibilityCoroutine);

        if (respawnCoroutine != null)
            StopCoroutine(respawnCoroutine);
    }
}