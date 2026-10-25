using UnityEngine;
using Unity.Netcode;
using TMPro;

public class TimerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;

    private MatchManager matchManager;

    private void Start()
    {
        if (timerText == null)
            timerText = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (timerText == null)
            return;

        if (NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsClient)
        {
            timerText.text = "Time: 60";
            return;
        }

        if (matchManager == null || !matchManager.IsSpawned)
        {
            matchManager = FindFirstObjectByType<MatchManager>();
            return;
        }

        int seconds = Mathf.CeilToInt(
            matchManager.TimeRemaining.Value
        );

        timerText.text = "Time: " + seconds;
    }
}