using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class MatchManager : NetworkBehaviour
{
    [SerializeField] private float matchDuration = 60f;

    public NetworkVariable<float> TimeRemaining =
        new NetworkVariable<float>(
            60f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public NetworkVariable<bool> IsMatchActive =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public NetworkVariable<FixedString128Bytes> MatchResult =
        new NetworkVariable<FixedString128Bytes>(
            new FixedString128Bytes(""),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private float timer;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        timer = matchDuration;
        TimeRemaining.Value = matchDuration;
        IsMatchActive.Value = false;
        MatchResult.Value = new FixedString128Bytes("");

        NetworkManager.OnClientConnectedCallback += OnClientConnected;
        CheckPlayersAndStartMatch();
    }

    private void OnClientConnected(ulong clientId)
    {
        if (IsServer)
            CheckPlayersAndStartMatch();
    }

    private void CheckPlayersAndStartMatch()
    {
        if (!IsServer || IsMatchActive.Value)
            return;

        if (NetworkManager.ConnectedClientsIds.Count >= 2)
        {
            timer = matchDuration;
            TimeRemaining.Value = matchDuration;
            MatchResult.Value = new FixedString128Bytes("");
            IsMatchActive.Value = true;

            Debug.Log("Both players connected! Match started.");
        }
    }

    private void Update()
    {
        if (!IsServer || !IsMatchActive.Value)
            return;

        timer -= Time.deltaTime;
        TimeRemaining.Value = Mathf.Max(0f, timer);

        if (timer <= 0f)
            EndMatch();
    }

    private void EndMatch()
    {
        if (!IsServer || !IsMatchActive.Value)
            return;

        // Stop the match first.
        IsMatchActive.Value = false;
        TimeRemaining.Value = 0f;

        var clients = NetworkManager.ConnectedClientsIds;

        if (clients.Count < 2)
        {
            MatchResult.Value =
                new FixedString128Bytes("Match ended: not enough players.");
            Debug.Log(MatchResult.Value.ToString());
            return;
        }

        PlayerHealth playerA =
            NetworkManager.ConnectedClients[clients[0]]
            .PlayerObject?.GetComponent<PlayerHealth>();

        PlayerHealth playerB =
            NetworkManager.ConnectedClients[clients[1]]
            .PlayerObject?.GetComponent<PlayerHealth>();

        if (playerA == null || playerB == null)
        {
            MatchResult.Value =
                new FixedString128Bytes("Match ended: player data unavailable.");
            Debug.Log(MatchResult.Value.ToString());
            return;
        }

        int killsA = playerA.KillCount.Value;
        int killsB = playerB.KillCount.Value;

        if (killsA > killsB)
        {
            MatchResult.Value = new FixedString128Bytes(
                "Player " + clients[0] + " Wins!"
            );
        }
        else if (killsB > killsA)
        {
            MatchResult.Value = new FixedString128Bytes(
                "Player " + clients[1] + " Wins!"
            );
        }
        else
        {
            MatchResult.Value = new FixedString128Bytes("Draw!");
        }

        Debug.Log("Match Over! " + MatchResult.Value.ToString());
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager != null)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}