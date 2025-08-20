using UnityEngine;
using UnityEngine.UI;

public class StartPanelLevel : MonoBehaviour
{
    [SerializeField] private Image _panel;
    [SerializeField] private Button _start;
    [SerializeField] private LevelEntryPoint _levelEntryPoint;

    private void Awake()
    {
        Show();
    }

    public void Show()
    => _panel.gameObject.SetActive(true);

    private void Hide()
    => _panel.gameObject.SetActive(false);

    private void OnEnable()
    {
        _start.onClick.AddListener(EnterGame);
    }

    private void OnDisable()
    {
        _start.onClick.RemoveListener(EnterGame);
    }

    private void EnterGame()
    {
        Hide();
        _levelEntryPoint.EnterGame();
    }
}