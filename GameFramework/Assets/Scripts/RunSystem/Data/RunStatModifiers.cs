using System;

[Serializable]
public class RunStatModifiers
{
    public int bonusHealth;
    public float bonusAttack;

    public float bonusMoveSpeed;
    public float bonusStamina;

    public float bonusCriticalChance;
    public float bonusCriticalDamage;

    public float bonusSlash;
    public float bonusPierce;
    public float bonusBlunt;

    public void Reset()
    {
        bonusHealth = 0;
        bonusAttack = 0f;

        bonusMoveSpeed = 0f;
        bonusStamina = 0f;

        bonusCriticalChance = 0f;
        bonusCriticalDamage = 0f;

        bonusSlash = 0f;
        bonusPierce = 0f;
        bonusBlunt = 0f;
    }
}