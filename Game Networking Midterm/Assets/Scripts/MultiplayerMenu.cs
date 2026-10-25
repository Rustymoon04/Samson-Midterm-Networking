using UnityEngine;
using Unity.Netcode;

public class MultiplayerMenu : MonoBehaviour
{
    [Header("Connection UI")]
    [SerializeField] private GameObject hostButton;
    [SerializeField] private GameObject clientButton;

    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }

        // Hide the buttons if a match is already active.
        CheckMatchStarted();
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    private void OnClientConnected(ulong clientId)
    {
        CheckMatchStarted();
    }

    private void Update()
    {
        CheckMatchStarted();
    }

    private void CheckMatchStarted()
    {
        MatchManager match = FindFirstObjectByType<MatchManager>();

        if (match != null && match.IsSpawned && match.IsMatchActive.Value)
        {
            if (hostButton != null)
                hostButton.SetActive(false);

            if (clientButton != null)
                clientButton.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}