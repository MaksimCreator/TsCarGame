using Ashsvp;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public abstract class VehicleFactory : MonoBehaviour
{
    private readonly Dictionary<SimcadeVehicleController, GameObject> _views = new();

    public SimcadeVehicleController CreatCar(Car car,Vector3 position,Vector3 rotation)
    {
        GameObject gameObject = Instantiate(GetTemplay(car).gameObject, position, Quaternion.Euler(rotation));
        SimcadeVehicleController simcadeVehicleController = gameObject.GetComponent<SimcadeVehicleController>();
        SystemGear gearSystem = gameObject.GetComponent<SystemGear>();

        gearSystem.BindCar(car);
        car.BindController(simcadeVehicleController);

        _views.Add(simcadeVehicleController, gameObject);

        return simcadeVehicleController;
    }

    public void Destroy(SimcadeVehicleController controller)
    {
        GameObject gameObject = _views[controller];
        _views.Remove(controller);
        Destroy(gameObject);
    }

    protected abstract SimcadeVehicleController GetTemplay(Car car);
}
