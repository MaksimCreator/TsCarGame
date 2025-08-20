using UnityEngine;
using Zenject;

public class LevelEntryPoint : MonoBehaviour
{
    [SerializeField] private Transform _pointSpawn;
    [SerializeField] private LevelLoop _levelLool;

    private BootstrapState _bootstrap;

    [Inject]
    private void Construct(BootstrapState bootstrapState)
    {
        _bootstrap = bootstrapState;
    }

    private void Awake()
    {
        _bootstrap.Init(_pointSpawn);
    }

    public void EnterGame()
    {
        _bootstrap.Enter();
        _levelLool.enabled = true;
    }
}