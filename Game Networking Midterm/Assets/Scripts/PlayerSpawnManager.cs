using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class PlayerSpawnManager : NetworkBehaviour
{
    [Header("Player Spawn Points")]
    [SerializeField] private Transform player1Spawn;
    [SerializeField] private Transform player2Spawn;

    private readonly HashSet<ulong> positionedPlayers = new();

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        NetworkManager.OnClientConnectedCallback += OnClientConnected;

        foreach (ulong clientId in NetworkManager.ConnectedClientsIds)
        {
            PositionPlayer(clientId);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (IsServer)
            PositionPlayer(clientId);
    }

    private void PositionPlayer(ulong clientId)
    {
        if (positionedPlayers.Contains(clientId))
            return;

        if (player1Spawn == null || player2Spawn == null)
        {
            Debug.LogError("Assign both player spawn points!");
            return;
        }

        if (!NetworkManager.ConnectedClients.TryGetValue(
            clientId, out var client))
            return;

        NetworkObject player = client.PlayerObject;

        if (player == null)
            return;

        PlayerSpawnHandler handler =
            player.GetComponent<PlayerSpawnHandler>();

        if (handler == null)
        {
            Debug.LogError("PlayerSpawnHandler is missing from NetworkPlayer!");
            return;
        }

        Transform spawn = GetSpawnPoint(clientId);

        handler.SetSpawnPositionClientRpc(
            spawn.position,
            spawn.rotation,
            new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { clientId }
                }
            });

        positionedPlayers.Add(clientId);

        Debug.Log("Player " + clientId + " assigned to spawn point.");
    }

    public void RespawnPlayer(ulong clientId)
    {
        if (!IsServer)
            return;

        if (player1Spawn == null || player2Spawn == null)
        {
            Debug.LogError("Assign both player spawn points!");
            return;
        }

        if (!NetworkManager.ConnectedClients.TryGetValue(
            clientId, out var client))
            return;

        NetworkObject player = client.PlayerObject;

        if (player == null)
            return;

        PlayerSpawnHandler handler =
            player.GetComponent<PlayerSpawnHandler>();

        if (handler == null)
        {
            Debug.LogError("PlayerSpawnHandler is missing!");
            return;
        }

        Transform spawn = GetSpawnPoint(clientId);

        handler.SetSpawnPositionClientRpc(
            spawn.position,
            spawn.rotation,
            new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { clientId }
                }
            });

        Debug.Log("Respawning player " + clientId);
    }

    private Transform GetSpawnPoint(ulong clientId)
    {
        return clientId == NetworkManager.ServerClientId
            ? player1Spawn
            : player2Spawn;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager != null)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}