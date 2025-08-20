using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Config/Level")]
public class LevelConfig : ScriptableObject
{
    [SerializeField] private int _countAiCar = 0;
    [SerializeField] private float _startRotaionCar;

    public int PlayerCar => 1;

    public int CountAiCar => _countAiCar;
    public float StartRotaionCar => _startRotaionCar;
}