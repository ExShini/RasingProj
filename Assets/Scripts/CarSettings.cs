using UnityEngine;

[CreateAssetMenu(fileName = "CarSettings", menuName = "Car/Car Settings")]
public class CarSettings : ScriptableObject
{
    public float EnginePower;
    public AnimationCurve MaxRoatationAngle;

    [Header("Сцепление с дорогой")]
    public AnimationCurve RoadOffsetLinerDamping;

    [Header("Сопротивление воздуха")]
    public AnimationCurve CarDampingByAngle;
    public AnimationCurve DragBySpeed;

    [Header("Перенаправление импульса при повороте")]
    [Tooltip("Доля боковой скорости, перенаправляемая вперёд за секунду (по скорости машины). 0 = нет перенаправления, 1 = мгновенное")]
    public AnimationCurve TurnMomentumRedirect;
}