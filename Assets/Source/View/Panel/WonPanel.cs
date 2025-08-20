using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WonPanel : MonoBehaviour
{
    [SerializeField] private Image _panel;
    [SerializeField] private Button _replay;
    [SerializeField] private TextMeshProUGUI _countAttempts;

    public void Show(Action onReplay,int countAttempts)
    {
        _panel.gameObject.SetActive(true);
        _replay.onClick.AddListener(() => onReplay.Invoke());
        _countAttempts.text = $"Race: {countAttempts}";
    }

    public void Hide()
    {
        _panel.gameObject.SetActive(false);
        _replay.onClick.RemoveAllListeners();
    }
}
