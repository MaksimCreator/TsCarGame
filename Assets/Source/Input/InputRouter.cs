using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputRouter : IInputRouter,IUpdateble
{
    private const int APPLAY_DRIFT = 1;

    private readonly CarInput _input = new();

    private Vector2 _directionMove;
    private float _accaleration;
    private float _rotate;

    public event Action<float> OnAccaleration;
    public event Action<float> OnRotate;
    public event Action<float> onDownSpace;
    public event Action onReset;

    public void Update(float delta)
    {
        OnMove(_input.Car.Movement);

        if (CanDrift())
            OnDrift();
    }

    public void Enable()
    {
        _input.Enable();

        _input.Car.Reset.performed += OnResete;
    }

    public void Disable()
    {
        _input.Disable();
        _input.Dispose();
    }

    private bool CanDrift()
    => _input.Car.Drift.phase == InputActionPhase.Performed;
    
    private void OnMove(InputAction obj)
    {
        _directionMove = obj.ReadValue<Vector2>();

        if (_directionMove == Vector2.zero)
            return;

        _accaleration = 0;
        _rotate = 0;

        if (_directionMove.y != 0)
            _accaleration = _directionMove.y > 0 ? 1 : -1;
        
        if(_directionMove.x != 0)
            _rotate = _directionMove.x > 0 ? 1 : -1;

        OnAccaleration.Invoke(_accaleration);
        OnRotate.Invoke(_rotate);
    }

    private void OnDrift()
    => onDownSpace.Invoke(APPLAY_DRIFT);

    private void OnResete(InputAction.CallbackContext obj)
    => onReset.Invoke();
}