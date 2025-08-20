using System;
using System.Collections.Generic;

[Serializable]
public class PathCar
{
    private readonly InputValue _inputValue = new();

    private readonly List<float> _accalerationInputs = new();
    private readonly List<float> _rotationInputs = new();
    private readonly List<float> _brakeInputs = new();
    
    private int _index;

    private int _lenght => _accalerationInputs.Count;

    public void AddPoint(float accalerationInput, float rotationInput, float brakeInput)
    {
        _accalerationInputs.Add(accalerationInput);
        _rotationInputs.Add(rotationInput);
        _brakeInputs.Add(brakeInput);
    }

    public bool TryGetPoint(out IInputValueProvider provider)
    {
        if (_index < 0 || _accalerationInputs.Count != _rotationInputs.Count || _accalerationInputs.Count != _brakeInputs.Count
            || _rotationInputs.Count != _brakeInputs.Count || _index > _lenght)
            throw new InvalidOperationException();

        provider = default;

        if (_index == _lenght)
            return false;

        _inputValue.SetAccelerationInput(_accalerationInputs[_index]);
        _inputValue.SetRotationInput(_rotationInputs[_index]);
        _inputValue.SetBrakeInput(_brakeInputs[_index]);

        _index++;
        provider = _inputValue;
        return true;
    }

    public void ResetIndex()
    => _index = 0;
}