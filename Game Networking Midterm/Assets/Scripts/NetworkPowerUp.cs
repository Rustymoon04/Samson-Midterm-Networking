using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class NetworkPowerUp : NetworkBehaviour
{
    public enum PowerUpType
    {
        DamageBoost,
        Invincibility
    }

    [Header("Power-Up Settings")]
    [SerializeField] private PowerUpType powerUpType;
    [SerializeField] private float effectDuration = 10f;
    [SerializeField] private int boostedDamage = 50;

    [Header("Pickup Settings")]
    [SerializeField] private float respawnDelay = 10f;

    private NetworkVariable<bool> isAvailable =
        new NetworkVariable<bool>(
            true,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private Collider pickupCollider;
    private Renderer[] pickupRenderers;

    public override void OnNetworkSpawn()
    {
        pickupCollider = GetComponent<Collider>();
        pickupRenderers = GetComponentsInChildren<Renderer>(true);

        isAvailable.OnValueChanged += OnAvailabilityChanged;
        UpdatePickupState(isAvailable.Value);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || !IsSpawned || !isAvailable.Value)
            return;

        PlayerHealth player =
            other.GetComponentInParent<PlayerHealth>();

        if (player == null || !player.IsSpawned || player.IsDead.Value)
            return;

        if (powerUpType == PowerUpType.DamageBoost)
        {
            player.ActivateDamagePowerUp(
                effectDuration, boostedDamage);
        }
        else
        {
            player.ActivateInvincibility(effectDuration);
        }

        isAvailable.Value = false;
        StartCoroutine(RespawnPickup());
    }

    private IEnumerator RespawnPickup()
    {
        yield return new WaitForSeconds(respawnDelay);

        if (IsServer && IsSpawned)
            isAvailable.Value = true;
    }

    private void OnAvailabilityChanged(bool oldValue, bool newValue)
    {
        UpdatePickupState(newValue);
    }

    private void UpdatePickupState(bool available)
    {
        if (pickupCollider != null)
            pickupCollider.enabled = available;

        if (pickupRenderers != null)
        {
            foreach (Renderer item in pickupRenderers)
                item.enabled = available;
        }
    }

    public override void OnNetworkDespawn()
    {
        isAvailable.OnValueChanged -= OnAvailabilityChanged;
    }
}