using System;

[Serializable]
public class LevelData
{
    public int CountAttempts { get; private set; } = 0; // попытка засчитывается только если игрок финишировал

    public bool IsCreatGhost => CountAttempts > 0;

    public void AddAttempts()
    => CountAttempts++;
}
