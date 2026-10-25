using UnityEngine;
using Unity.Netcode;
using System;

public class PlayerShooting : NetworkBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private float fireRate = 0.25f;
    [SerializeField] private float shootRange = 100f;
    [SerializeField] private int damage = 25;

    private float nextFireTime;
    private Camera playerCamera;
    private PlayerHealth playerHealth;

    private void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (!IsSpawned || !IsOwner)
            return;

        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        if (playerHealth == null || playerHealth.IsDead.Value)
            return;

        MatchManager matchManager =
            FindFirstObjectByType<MatchManager>();

        if (matchManager == null ||
            !matchManager.IsSpawned ||
            !matchManager.IsMatchActive.Value)
            return;

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        if (playerCamera == null)
            return;

        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;

            ShootServerRpc(
                playerCamera.transform.position,
                playerCamera.transform.forward
            );
        }
    }

    [ServerRpc]
    private void ShootServerRpc(
        Vector3 origin,
        Vector3 direction,
        ServerRpcParams rpcParams = default)
    {
        if (!IsSpawned || direction.sqrMagnitude < 0.9f)
            return;

        // Verify that the RPC came from this player's owner.
        if (rpcParams.Receive.SenderClientId != OwnerClientId)
            return;

        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        if (playerHealth == null || playerHealth.IsDead.Value)
            return;

        MatchManager matchManager =
            FindFirstObjectByType<MatchManager>();

        if (matchManager == null ||
            !matchManager.IsSpawned ||
            !matchManager.IsMatchActive.Value)
            return;

        Camera serverCamera = GetComponentInChildren<Camera>();

        if (serverCamera != null &&
            Vector3.Distance(
                origin, serverCamera.transform.position) > 2.5f)
            return;

        Ray ray = new Ray(origin, direction.normalized);
        RaycastHit[] hits = Physics.RaycastAll(ray, shootRange);

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            NetworkObject hitNetworkObject =
                hit.collider.GetComponentInParent<NetworkObject>();

            // Ignore this player's own colliders.
            if (hitNetworkObject != null &&
                hitNetworkObject.OwnerClientId == OwnerClientId)
                continue;

            PlayerHealth targetHealth =
                hit.collider.GetComponentInParent<PlayerHealth>();

            if (targetHealth != null)
            {
                if (targetHealth.IsDead.Value)
                    return;

                int actualDamage = playerHealth.CurrentDamage.Value;

                // Fallback if the configured default damage is needed.
                if (actualDamage <= 0)
                    actualDamage = damage;

                targetHealth.TakeDamage(actualDamage, OwnerClientId);
                return;
            }

            // The shot hit scenery or another non-player object.
            return;
        }
    }
}