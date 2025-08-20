using Ashsvp;
using System;
using UnityEngine;

public class CarFactory : VehicleFactory
{
    [SerializeField] private SimcadeVehicleController _playerCar;
    [SerializeField] private SimcadeVehicleController _gosteCar;

    protected override SimcadeVehicleController GetTemplay(Car car)
    {
        if(car is PlayerCar)
            return _playerCar;
        else if(car is Ghost)
            return _gosteCar;

        throw new InvalidOperationException();
    }
}
