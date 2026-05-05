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
        if (WayPoints == null || WayPoints.Length == 0) return;

        // 1. ���������� ������� ����� (��������� �� ������)
        Vector3 targetPoint = WayPoints[nextWayPointInd].Position;
        targetPoint.y = 0;
        currentPosition.y = 0;

        Vector3 directionToTarget = (targetPoint - currentPosition).normalized;

        // ������� ���� � �������� (�� -180 �� 180)
        float angle = Vector3.SignedAngle(forwardV.normalized, directionToTarget, Vector3.up);
        AngleRotation = angle;

        // ������ ��� �� ������
        Acceleration = 1f;

    }
}