using UnityEngine;

public abstract class CarProcessor : ScriptableObject
{
    // значения от -0.5f до 1f
    public float Acceleration { get; protected set; }
    
    // ограничен настройками машины
    public float AngleRotation { get; protected set; }


    // чекпоинты трассы
    public RoadWayPoint[] WayPoints;
    // детализированные точки дороги
    public Vector3[] DetailRoadPoints;

    // количество "точек дороги" между каждыми двумя чекпоинтами
    public int RaodResolution;

    public float RoadWidth;
    public float MaxRotationAngle;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="currentPosition">Текущая позиция машины</param>
    /// <param name="currentSpeed">Вектор скорости машины</param>
    /// <param name="forwardV">Направление вперёд машины</param>
    /// <param name="nextWayPointInd">Индекс следующего чек поинта трассы</param>
    /// <param name="closestRoadPoint">Ближайшая точка трассы</param>
    public abstract void Process(
        Vector3 currentPosition,
        Vector3 currentSpeed,
        Vector3 forwardV,
        int nextWayPointInd,
        Vector3 closestRoadPoint);
}