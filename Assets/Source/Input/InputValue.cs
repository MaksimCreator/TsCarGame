public class InputValue : IInputValueProvider
{
    private float _accelerationInput;
    private float _rotationInput;
    private float _brakeInput;

    public void SetAccelerationInput(float value)
    => _accelerationInput = value;

    public void SetRotationInput(float value)
    => _rotationInput = value;

    public void SetBrakeInput(float value)
    => _brakeInput = value;

    public float GetAccelerationInput()
    => GetValue(ref _accelerationInput);

    public float GetSteerInput()
    => GetValue(ref _rotationInput);

    public float GetBrakeInput()
    => GetValue(ref _brakeInput);

    private float GetValue(ref float inputValue)
    {
        float value = inputValue;
        inputValue = 0;
        return value;
    }
}
