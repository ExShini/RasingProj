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
    public float RechargeDuration { get; private set; }
    public float ActiveTime { get; private set; }
    public float ActiveDuration { get; private set; }
    public float PowerMultiplier { get; private set; }

    public AbilityData(AbilityType type, int activationNum, float rechDuration,
                       float activeDuration, float powerMultiplier)
    {
        Type = type;
        ActivationNum = activationNum;
        RechargeDuration = rechDuration;
        ActiveDuration = activeDuration;
        PowerMultiplier = powerMultiplier;
    }

    public void AddActivations(int activationNumToAdd)
    {
        ActivationNum += activationNumToAdd;
    }

    public void Activate()
    {
        ActivationNum--;
        ActiveTime = ActiveDuration;
        RechargeTime = RechargeDuration;
    }

    public void Update(float deltaTime)
    {
        if (ActiveTime > 0)
        {
            ActiveTime -= deltaTime;
            if (ActiveTime < 0)
                ActiveTime = 0;
            return;
        }

        if (RechargeTime > 0)
        {
            RechargeTime -= deltaTime;
            if (RechargeTime < 0)
                RechargeTime = 0;
        }
    }

    public bool IsActive { get { return ActiveTime > 0; } }
    public bool IsReady { get { return RechargeTime <= 0 && ActivationNum > 0; } }
}

public class CarAbilityController
{
    private const int NitroStartNum = 2;
    private const float NitroRecharge = 5f;
    private const float NitroDuration = 1.5f;
    private const float NitroPowerMultiplier = 2.5f;

    private List<AbilityData> _abilities;
    private Dictionary<AbilityType, AbilityData> _abilitiesByType;

    public CarAbilityController()
    {
        _abilities = new List<AbilityData>()
        {
            new AbilityData(AbilityType.Nitro, NitroStartNum, NitroRecharge,
                            NitroDuration, NitroPowerMultiplier),
        };

        _abilitiesByType = new Dictionary<AbilityType, AbilityData>();
        foreach (var ability in _abilities)
        {
            _abilitiesByType[ability.Type] = ability;
        }
    }

    public void Update(float deltaTime)
    {
        foreach (var ability in _abilities)
        {
            ability.Update(deltaTime);
        }
    }

    public bool UseAbility(AbilityType abType)
    {
        if (!_abilitiesByType.TryGetValue(abType, out var abData))
            return false;

        if (!abData.IsReady)
            return false;

        abData.Activate();
        return true;
    }

    public bool IsAbilityActive(AbilityType abType)
    {
        return _abilitiesByType.TryGetValue(abType, out var abData) && abData.IsActive;
    }

    public bool IsAbilityReady(AbilityType abType)
    {
        return _abilitiesByType.TryGetValue(abType, out var abData) && abData.IsReady;
    }

    public float GetPowerMultiplier()
    {
        float multiplier = 1f;
        foreach (var ability in _abilities)
        {
            if (ability.IsActive && ability.PowerMultiplier > multiplier)
            {
                multiplier = ability.PowerMultiplier;
            }
        }
        return multiplier;
    }

    public int GetActivationsLeft(AbilityType abType)
    {
        return _abilitiesByType.TryGetValue(abType, out var abData) ? abData.ActivationNum : 0;
    }

    public float GetRechargeProgress(AbilityType abType)
    {
        if (!_abilitiesByType.TryGetValue(abType, out var abData) || abData.RechargeDuration <= 0)
            return 1f;
        return 1f - (abData.RechargeTime / abData.RechargeDuration);
    }
}
