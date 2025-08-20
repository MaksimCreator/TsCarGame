using System;

public class CarTrajectoryRecorder : IUpdateble,IControl
{
    private readonly LevelData _levelData;
    
    private PathCar _newPath = new();
    private float _timeNewPath;

    private PathCar _shortestPath;
    private float _timeShortestPath;

    private bool _isEnable;

    public CarTrajectoryRecorder(LevelData levelData)
    {
        _levelData = levelData;
    }

    public void SaveValueTick(float acceleration,float rotation,float brakeInput )
    {
        if (_levelData.CountAttempts < 0)
            throw new InvalidOperationException();

        _newPath.AddPoint(acceleration, rotation, brakeInput);
    }

    public void EndTrajectory()
    {
        if (_shortestPath == null || _timeNewPath < _timeShortestPath)
        {
            _shortestPath = _newPath;
            _timeShortestPath = _timeNewPath;
        }

        _timeNewPath = 0;
        _newPath = new();
        Disable();
        _shortestPath.ResetIndex();
    }

    public void Update(float delta)
    {
        if (_isEnable == false)
            return;

        _timeNewPath += delta;
    }

    public void Enable()
    {
        _isEnable = true;
    }

    public void Disable()
    {
        _isEnable = false;
    }

    public bool TryGetTarjectory(out IInputValueProvider inputProvider)
    {
        inputProvider = default;

        if (_shortestPath == null)
            return false;

        return _shortestPath.TryGetPoint(out inputProvider);
    }
}
