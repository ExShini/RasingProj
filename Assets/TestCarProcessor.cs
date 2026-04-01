using UnityEngine;


[CreateAssetMenu(fileName = "TestCarProcessor", menuName = "Car/Test Car Processor")]
public class TestCarProcessor : CarProcessor
{
    public override void Process(
        Vector3 currentPosition,
        Vector3 currentSpeed,
        Vector3 forwardV,
        int nextWayPointInd,
        Vector3 closestRoadPoint)
    {
        if (Points == null || Points.Length == 0) return;

        // 1. Определяем целевую точку (следующую по списку)
        Vector3 targetPoint = Points[nextWayPointInd].Position;
        targetPoint.y = 0;
        currentPosition.y = 0;

        Vector3 directionToTarget = (targetPoint - currentPosition).normalized;

        // Считаем угол в градусах (от 0 до 180)
        float angle = Vector3.SignedAngle(forwardV.normalized, directionToTarget, Vector3.up);
        AngleRotation = angle;

        // Полный газ на прямой
        Acceleration = 1f;

    }
}