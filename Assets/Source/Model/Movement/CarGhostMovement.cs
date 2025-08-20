using Zenject;

public class CarGhostMovement : IFixedUpdateble
{
    private readonly CarTrajectoryRecorder _carTrajectory;
    private readonly Ghost _ghost;

    private IInputValueProvider _provider;

    [Inject]
    public CarGhostMovement(CarTrajectoryRecorder levelData,Ghost car)
    {
        _carTrajectory = levelData;
        _ghost = car;
    }

    public void FixedUpdate()
    {
        if(_carTrajectory.TryGetTarjectory(out _provider))
            _ghost.Move(_provider.GetAccelerationInput(),_provider.GetSteerInput(),_provider.GetBrakeInput());
    }
}