using UnityEngine;
using Unity.Netcode;
using TMPro;

public class MatchResultUI : MonoBehaviour
{
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultText;

    private MatchManager matchManager;

    private void Start()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    private void Update()
    {
        if (matchManager == null || !matchManager.IsSpawned)
        {
            matchManager = FindFirstObjectByType<MatchManager>();
            return;
        }

        string result = matchManager.MatchResult.Value.ToString();

        if (!string.IsNullOrEmpty(result))
        {
            if (resultPanel != null && !resultPanel.activeSelf)
                resultPanel.SetActive(true);

            if (resultText != null)
                resultText.text = result;
        }
    }
}
