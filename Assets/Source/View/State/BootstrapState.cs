using Ashsvp;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BootstrapState // занимается инициализацией уровня в методе Enter
{
    private readonly CarFactory _factory;
    private readonly LevelData _levelData;
    private readonly LevelConfig _levelConfig;
    private readonly PlayerCar _playerCar;
    private readonly Ghost _ghost;

    private List<SimcadeVehicleController> _carsLevel = new();
    private Transform _positionSpawnCar;

    private Vector3 _startRotaionCar => Vector3.up * _levelConfig.StartRotaionCar;

    [Inject]
    public BootstrapState(CarFactory factory, 
        LevelData levelData,
        LevelConfig levelConfig,
        PlayerCar playerCar,
        Ghost ghost)
    {
        _levelConfig = levelConfig;
        _levelData = levelData;
        _playerCar = playerCar;
        _factory = factory;
        _ghost = ghost;
    }

    public void Init(Transform points)
    => _positionSpawnCar = points;

    public void Enter()
    {
        if (_carsLevel.Count != 0)
            DestroyCars(_carsLevel);

        CreatCars();
    }

    private void DestroyCars(List<SimcadeVehicleController> cars)
    {
        for (int i = 0; i < cars.Count; i++)
            _factory.Destroy(cars[i]);

        _carsLevel.Clear();
    }

    private void CreatCars()
    {
        Vector3 rotationCar = _startRotaionCar;
        _carsLevel.Add(_factory.CreatCar(_playerCar, _positionSpawnCar.position, rotationCar));

        if (_levelData.IsCreatGhost)
            _carsLevel.Add(_factory.CreatCar(_ghost, _positionSpawnCar.position, rotationCar));
    }
}