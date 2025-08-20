using UnityEngine;
using Zenject;

public class LevelInstaller : MonoInstaller
{
    [SerializeField] private LevelEntryPoint _levelEntryPoint;
    [SerializeField] private StartPanelLevel _startPanelLevel;
    [SerializeField] private LevelConfig _levelConfig;
    [SerializeField] private CarFactory _carFactory;
    [SerializeField] private WonPanel _wonPanel;
    [SerializeField] private WonTriger _triger;

    public override void InstallBindings()
    {
        RegistarySO();
        RegistaryInput();
        RegistratyData();
        RegistaryState();
        RegistaryPanel();
        RegistaryEntite();
        RegistaryTriger();
        RegistaryFactory();
        RegistaryMovement();
        RegistaryController();
        RegistaryEntryPoint();
        RegistratyTrajectory();
    }

    private void RegistaryEntryPoint()
    {
        Container.Bind<LevelEntryPoint>()
            .FromInstance(_levelEntryPoint)
            .AsSingle();
    }

    private void RegistaryInput()
    {
        Container.Bind<IInputRouter>()
            .To<InputRouter>()
            .FromNew()
            .AsSingle();
    }

    private void RegistaryEntite()
    {
        Container.Bind<PlayerCar>()
            .FromNew()
            .AsSingle();

        Container.Bind<Ghost>()
            .FromNew()
            .AsSingle();
    }

    private void RegistaryMovement()
    {
        Container.Bind<CarMovement>()
            .FromNew()
            .AsSingle();

        Container.Bind<CarGhostMovement>()
            .FromNew()
            .AsSingle();
    }

    private void RegistratyTrajectory()
    {
        Container.Bind<CarTrajectoryRecorder>()
            .FromNew()
            .AsSingle();
    }

    private void RegistratyData()
    {
        Container.Bind<LevelData>()
            .FromNew()
            .AsSingle();
    }

    private void RegistarySO()
    {
        Container.Bind<LevelConfig>()
            .FromNew()
            .AsSingle();
    }

    private void RegistaryState()
    {
        Container.Bind<BootstrapState>()
            .FromNew()
            .AsSingle();
    }

    private void RegistaryFactory()
    {
        Container.Bind<CarFactory>()
            .FromInstance(_carFactory)
            .AsSingle();
    }

    private void RegistaryTriger()
    {
        Container.Bind<WonTriger>()
            .FromInstance(_triger)
            .AsSingle();
    }

    private void RegistaryPanel()
    {
        Container.Bind<WonPanel>()
            .FromInstance(_wonPanel)
            .AsSingle();

        Container.Bind<StartPanelLevel>()
            .FromInstance(_startPanelLevel)
            .AsSingle();
    }

    private void RegistaryController()
    {
        Container.Bind<CarController>()
            .FromNew()
            .AsSingle();

        Container.Bind<GameController>()
            .FromNew()
            .AsSingle();
    }
}
