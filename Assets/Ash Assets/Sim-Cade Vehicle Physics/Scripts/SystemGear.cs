using Ashsvp;
using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class SystemGear : MonoBehaviour
{
    private Car _car;

    public float carSpeed;
    public int currentGear = 1;
    public int[] gearSpeeds = new int[] { 40, 80, 120, 160, 220 };

    public AudioSystem AudioSystem;

    private int currentGearTemp;

    public event Action OnGearChange;
        
    public int CurrntGearProperty
    {
        get
        {
            return currentGearTemp;
        }

        set
        {
            currentGearTemp = value;

            if(_car.AccelerationInput > 0 && _car.LocalVehicleVelocity.z > 0 && AudioSystem.GearSound.isPlaying == false && _car.IsGrounded)
            {
                OnGearChange?.Invoke();
                AudioSystem.GearSound.Play();
                shiftingGear();
            }

            AudioSystem.engineSound.volume = 0.5f;
        }
    }

    public SystemGear BindCar(Car car)
    { 
        _car = car;
        return this;
    }

    private void Update()
    {
        carSpeed = Mathf.RoundToInt(_car.Speed); //car speed in Km/hr

        GearShift();
    }

    private void GearShift()
    {
        for (int i = 0; i < gearSpeeds.Length; i++)
        {
            if (carSpeed > gearSpeeds[i])
            {
                currentGear = i + 1;
            }
            else break;
        }
        if (CurrntGearProperty != currentGear)
        {
            CurrntGearProperty = currentGear;
        }

    }

    private async UniTaskVoid shiftingGear()
    {
        _car.CanMove = false;
        await UniTask.Delay(300);
        _car.CanMove = true;
    }
}