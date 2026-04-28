using NUnit.Framework;
using System.Collections.Generic;

public enum AbilityType
{
    None,
    Nitro,
}


public class AbilityData
{
    public AbilityType Type;
    public int ActivationNum;
    public float RechargeTime;
    public float RechargeDuration;
}

public class CarAbilityController
{
    private const int NitroStartNum = 2;
    private const float NitroRecharge = 5f;
    private const float NitroDuration = 1.5f;


    private List<AbilityData> _abilities;
    private Dictionary<AbilityType, AbilityData> _abilitiesByType;

    public CarAbilityController()
    {
        _abilities = new List<AbilityData>()
        {
            new AbilityData()
            {
                Type = AbilityType.Nitro,
                ActivationNum = NitroStartNum,
                RechargeTime = 0,
                RechargeDuration = NitroRecharge,
            },
        };

        _abilitiesByType = new Dictionary<AbilityType, AbilityData>();
        foreach(var ability in _abilities)
        {
            _abilitiesByType[ability.Type] = ability;
        }
    }

    public void Update(float deltaTime)
    {

    }

    private void UpdateRecharge(float deltaTime)
    {
        for (int i = 0; i < _abilities.Count; i++)
        {
            var ability = _abilities[i];
            if(ability.RechargeTime > 0)
            {
                ability.RechargeTime -= deltaTime;
                if(ability.RechargeTime < 0)
                {
                    ability.RechargeTime = 0;
                }
            }
        }
    }

    public bool AbilityIsActive(AbilityType abType)
    {
        return false;
    }

    public bool AbilityReadyActive(AbilityType abType)
    {
        return false;
    }
}