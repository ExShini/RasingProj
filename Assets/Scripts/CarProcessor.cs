using UnityEngine;

public abstract class CarProcessor : ScriptableObject
{
    // Ускорение от -1f до 1f
    public float Acceleration { get; protected set; }
    
    // Максимальный угол поворота
    public float AngleRotation { get; protected set; }

    public CarAbilityController AbilityController { get; set; }


    // Опорные точки пути
    public RoadWayPoint[] WayPoints;
    // Детализированные точки пути
    public Vector3[] DetailRoadPoints;

    // Количество "точек пути" между двумя опорными
    public int RaodResolution;

    public float RoadWidth;
    public float MaxRotationAngle;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="currentPosition">Текущая позиция машины</param>
    /// <param name="currentSpeed">Вектор скорости машины</param>
    /// <param name="forwardV">Направление вперёд машины</param>
    /// <param name="nextWayPointInd">Индекс следующей опорной точки</param>
    /// <param name="closestRoadPoint">Ближайшая точка дороги</param>
    public abstract void Process(
        Vector3 currentPosition,
        Vector3 currentSpeed,
        Vector3 forwardV,
        int nextWayPointInd,
        Vector3 closestRoadPoint);
}
