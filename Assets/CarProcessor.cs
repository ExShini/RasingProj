using UnityEngine;

public abstract class CarProcessor : ScriptableObject
{
    // значения от -0.5f до 1f
    public float Acceleration { get; protected set; }
    
    // ограничен настройками машины
    public float AngleRotation { get; protected set; }



    public RoadPointsState[] Points;
    public float MaxRotationAngle;

    public abstract void Process(
        Vector3 currentPosition,
        Vector3 currentSpeed,
        Vector3 forwardV,
        int nextWayPointInd,
        Vector3 closestRoadPoint);
}