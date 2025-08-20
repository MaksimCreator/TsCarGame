using Ashsvp;
using UnityEngine;

public abstract class Car
{
    private Transform _transform;
    private SimcadeVehicleController _carSimcadeConrtoller;

    public bool CanMove = true;

    public float Speed => _carSimcadeConrtoller.localVehicleVelocity.magnitude * 3.6f;

    public float AccelerationInput => _carSimcadeConrtoller.AccelerationInput;

    public Vector3 LocalVehicleVelocity => _carSimcadeConrtoller.localVehicleVelocity;

    public bool IsGrounded => _carSimcadeConrtoller.vehicleIsGrounded;

    public void BindController(SimcadeVehicleController CarConrtoller)
    {
        _carSimcadeConrtoller = CarConrtoller;
        _transform = _carSimcadeConrtoller.transform;
    }

    public void Move(float acceleration, float rotaion, float brakeInput)
    {
        _carSimcadeConrtoller.ChangeForces(acceleration,rotaion,brakeInput);
    }

    public void Reset()
    => _transform.rotation = Quaternion.Euler(0, _transform.rotation.eulerAngles.y, 0);
}