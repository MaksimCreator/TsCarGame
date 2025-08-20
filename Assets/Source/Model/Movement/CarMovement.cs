using System;
using Zenject;

public class CarMovement : IFixedUpdateble,IControl,IUpdateble
{
    private readonly PlayerCar _car;
    private readonly InputValue _inputValueData = new();
    private readonly CarTrajectoryRecorder _carTrajectoryRecorder;

    private float _acceleration;
    private float _rotation;
    private float _brakeInput;

    public event Func<bool> canDrive;
    public event Func<bool> canAccelerate;

    [Inject]
    public CarMovement(PlayerCar car,CarTrajectoryRecorder carTrajectoryRecorder)
    {
        _car = car;
        _carTrajectoryRecorder = carTrajectoryRecorder;
    }

    private bool CanDrive()
    {
        if (canDrive == null)
            return true;

        if(_car.CanMove == false)
            return false;

        return CanAction(canAccelerate.GetInvocationList());
    }

    private bool CanAccelerate()
    {
        if (canAccelerate == null)
            return true;

        return CanAction(canAccelerate.GetInvocationList());
    }

    private bool CanAction(Delegate[] action)
    {
        for (int i = 0; i < action.Length; i++)
        {
            Func<bool> canAction = action[i] as Func<bool>;

            if (canAction.Invoke() == false)
                return false;
        }

        return true;
    }

    public void SetAccelerationInput(float value)
    => _inputValueData.SetAccelerationInput(value);

    public void SetRotationInput(float value)
    => _inputValueData.SetRotationInput(value);

    public void SetBrakeInput(float value)
    => _inputValueData.SetBrakeInput(value);

    public void FixedUpdate()
    {
        _acceleration = _inputValueData.GetAccelerationInput();
        _rotation = _inputValueData.GetSteerInput();
        _brakeInput = _inputValueData.GetBrakeInput();

        if (CanDrive() && CanAccelerate() == false)
        {
            _acceleration = 0;
        }
        else if((CanDrive() && CanAccelerate()) == false)
        {
            _acceleration = 0;
            _rotation = 0;
            _brakeInput = 1;
        }

        _carTrajectoryRecorder.SaveValueTick(_acceleration,_rotation,_brakeInput);
        _car.Move(_acceleration,_rotation,_brakeInput);
    }

    public void Enable()
    {
        _carTrajectoryRecorder.Enable();
    }

    public void Disable()
    {
        _carTrajectoryRecorder.Disable();
    }

    public void Update(float delta)
    {
        _carTrajectoryRecorder.Update(delta);
    }
}