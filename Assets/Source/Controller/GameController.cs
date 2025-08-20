public class GameController : IControl
{
    private readonly WonTriger _triger;
    private readonly CarController _carController;
    private readonly WonPanel _panel;
    private readonly LevelData _levelData;
    private readonly CarTrajectoryRecorder _carTrajectoryRecorder;
    private readonly BootstrapState _bootstrapState;

    public GameController(WonTriger triger,
        CarController carController,
        WonPanel panel,
        LevelData levelData,
        CarTrajectoryRecorder carTrajectoryRecorder,
        BootstrapState bootstrapState)
    {
        _triger = triger;
        _carController = carController;
        _panel = panel;
        _levelData = levelData;
        _carTrajectoryRecorder = carTrajectoryRecorder;
        _bootstrapState = bootstrapState;
    }

    public void Disable()
    {
        _triger.onWon -= OnEnterWonPanel;
    }

    public void Enable()
    {
        _triger.onWon += OnEnterWonPanel;
    }

    private void OnEnterWonPanel()
    {
        _levelData.AddAttempts();
        _carController.Disable();
        _carTrajectoryRecorder.EndTrajectory();
        _panel.Show(OnReplay,_levelData.CountAttempts);
    }

    private void OnReplay()
    {
        _panel.Hide();
        _bootstrapState.Enter();
        _carTrajectoryRecorder.Enable();
        _carController.Enable();
    }
}