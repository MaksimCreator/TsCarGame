using Zenject;

public class LevelLoop : GameLoop
{
    private IControl[] _controls;
    private IUpdateble[] _updatebles;
    private IFixedUpdateble[] _fixedUpdatebles;

    [Inject]
    private void Construct(
        GameController gameController,
        CarController carController)
    {
        _controls = new IControl[]
        {
            gameController,
            carController
        };

        _updatebles = new IUpdateble[]
        {
            carController
        };

        _fixedUpdatebles = new IFixedUpdateble[]
        {
            carController
        };
    }

    protected override IControl[] GetControls()
    => _controls;

    protected override IFixedUpdateble[] GetFixedUpdateble()
    => _fixedUpdatebles;

    protected override IUpdateble[] GetUpdatebles()
    => _updatebles;

    private void OnValidate()
    {
        if(enabled == true)
            enabled = false;
    }
}