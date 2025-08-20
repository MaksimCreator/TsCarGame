using System;

[Serializable]
public class LevelData
{
    public int CountAttempts { get; private set; } = 0; // ������� ������������� ������ ���� ����� �����������

    public bool IsCreatGhost => CountAttempts > 0;

    public void AddAttempts()
    => CountAttempts++;
}
