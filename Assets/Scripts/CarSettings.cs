using UnityEngine;

[CreateAssetMenu(fileName = "CarSettings", menuName = "Car/Car Settings")]
public class CarSettings : ScriptableObject
{
    public float EnginePower;
    public AnimationCurve MaxRoatationAngle;
    public AnimationCurve RoadOffsetLinerDamping;
    public AnimationCurve CarDampingByAngle;
}