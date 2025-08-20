public class CarController : IControl,IFixedUpdateble,IUpdateble
{
    private readonly IInputRouter _input;
    private readonly CarMovement _carMovement;
    private readonly CarGhostMovement _ghostMovement;
    private readonly PlayerCar _playerCar;

    private bool _isEnable;

    public CarController(IInputRouter input,
        CarMovement carMovement,
        CarGhostMovement carGhostMovement,
        PlayerCar playerCar)
    {
        _input = input;
        _carMovement = carMovement;
        _ghostMovement = carGhostMovement;
        _playerCar = playerCar;
    }

    public void Disable()
    {
        _input.Disable();
        _input.OnAccaleration -= _carMovement.SetAccelerationInput;
        _input.OnRotate -= _carMovement.SetRotationInput;
        _input.onDownSpace -= _carMovement.SetBrakeInput;
        _input.onReset -= _playerCar.Reset;

        _carMovement.Disable();

        _isEnable = false;
    }

    public void Enable()
    {
        _input.Enable();
        _input.OnAccaleration += _carMovement.SetAccelerationInput;
        _input.OnRotate += _carMovement.SetRotationInput;
        _input.onDownSpace += _carMovement.SetBrakeInput;
        _input.onReset += _playerCar.Reset;

        _carMovement.Enable();

        _isEnable = true;
    }

    public void FixedUpdate()
    {
        if (_isEnable == false)
            return;

        _carMovement.FixedUpdate();
        _ghostMovement.FixedUpdate();
    }

    public void Update(float delta)
    {
        if (_isEnable == false)
            return;

        _input.Update(delta);
        _carMovement.Update(delta);
    }
}