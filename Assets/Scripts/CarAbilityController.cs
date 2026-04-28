using NUnit.Framework;
using System.Collections.Generic;

public enum AbilityType
{
    None,
    Nitro,
}


public class AbilityData
{
    public AbilityType Type { get; private set; }
    public int ActivationNum { get; private set; }
    public float RechargeTime { get; private set; }
    public float RechargeDuration {  get; private set; }

    public AbilityData(AbilityType type, int activationNum, float rechDuration)
    {
        Type = type;
        ActivationNum = activationNum;
        RechargeDuration = rechDuration;
    }

    public void AddActivations(int activationNumToAdd)
    {
        ActivationNum += activationNumToAdd;
    }

    public void Recharge(float deltaTime)
    {
        RechargeTime -= deltaTime;
        if (RechargeTime < 0)
        {
            RechargeTime = 0;
        }
    }
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
            new AbilityData(AbilityType.Nitro, NitroStartNum, NitroRecharge),
            
        };

        _abilitiesByType = new Dictionary<AbilityType, AbilityData>();
        foreach(var ability in _abilities)
        {
            _abilitiesByType[ability.Type] = ability;
        }
    }

    public void Update(float deltaTime)
    {
        UpdateRecharge(deltaTime);
    }

    private void UpdateRecharge(float deltaTime)
    {
        for (int i = 0; i < _abilities.Count; i++)
        {
            var ability = _abilities[i];
            if(ability.RechargeTime > 0)
            {
                ability.Recharge(deltaTime);
            }
        }
    }

    public bool AbilityIsActive(AbilityType abType)
    {
        return false;
    }

    public bool AbilityIsReady(AbilityType abType)
    {
        if(_abilitiesByType.ContainsKey(abType) == false)
        {
            return false;
        }

        var abData = _abilitiesByType[abType];
        bool isReady = abData.RechargeTime <= 0 && abData.ActivationNum > 0;

        return isReady;
    }
}