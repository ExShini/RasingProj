using Mono.Cecil;
using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct RoadPointsState
{
    public int Ind;
    public bool Achived;
    public Vector3 Position;
}

public class Car : MonoBehaviour
{
    private const float AchiveDist = 1.5f;
    private const float AchiveDistSqr = AchiveDist * AchiveDist;


    public float EnginePower;
    public float MaxRoatationAngle;


    public CarProcessor Brain;

    public RoadPointsState NextPoint;

    private RoadPointsState[] _roadPoints;

    private Rigidbody _rb;
    private Transform _trans;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _trans = GetComponent<Transform>();
    }

    public void Init(Vector3[] roadPoints)
    {
        _roadPoints = new RoadPointsState[roadPoints.Length];
        for (int i = 0; i < roadPoints.Length; i++)
        {
            _roadPoints[i] = new RoadPointsState()
            {
                Ind = i,
                Achived = false,
                Position = roadPoints[i]
            };
        }

        if(Brain != null)
            Brain.Points = _roadPoints;
    }

    public void Process()
    {
        var currPosition = _trans.position;
        var closestPoint = GetClosestRoadPoint(currPosition);

        var nextWayPoint = _roadPoints.First(x => x.Achived == false);
        NextPoint = nextWayPoint;

        Brain.Process(currPosition, _rb.linearVelocity, transform.forward, nextWayPoint.Ind, closestPoint);
        UdpateCar();
    }

    public void CheckAchived()
    {
        var point = _roadPoints[NextPoint.Ind];

        var distVect = point.Position - this.transform.position;
        distVect.y = 0;
        if (distVect.sqrMagnitude <= AchiveDistSqr)
        {
            point.Achived = true;
            _roadPoints[NextPoint.Ind] = point;

            int nextPointInd = NextPoint.Ind + 1;
            if(nextPointInd >= _roadPoints.Length)
                nextPointInd = 0;

            NextPoint = _roadPoints[nextPointInd];
        }
    }


    public Vector3 GetClosestRoadPoint(Vector3 currentPos)
    {
        int closestIndex = 0;
        if (_roadPoints == null || _roadPoints.Length == 0) return currentPos;

        float minDistanceSqr = float.MaxValue;

        // 1. Ищем ближайший индекс в массиве
        for (int i = 0; i < _roadPoints.Length; i++)
        {
            float distSqr = (currentPos - _roadPoints[i].Position).sqrMagnitude;
            if (distSqr < minDistanceSqr)
            {
                minDistanceSqr = distSqr;
                closestIndex = i;
            }
        }

        // 2. Уточняем положение между сегментами (Проекция на отрезок)
        int prevIndex = (closestIndex + _roadPoints.Length - 1) % _roadPoints.Length;
        int nextIndex = (closestIndex + 1) % _roadPoints.Length;

        Vector3 p1 = ProjectPointOnLineSegment(_roadPoints[prevIndex].Position, _roadPoints[closestIndex].Position, currentPos);
        Vector3 p2 = ProjectPointOnLineSegment(_roadPoints[closestIndex].Position, _roadPoints[nextIndex].Position, currentPos);

        return (currentPos - p1).sqrMagnitude < (currentPos - p2).sqrMagnitude ? p1 : p2;
    }

    // Вспомогательная функция проекции точки на отрезок
    private Vector3 ProjectPointOnLineSegment(Vector3 start, Vector3 end, Vector3 point)
    {
        Vector3 line = end - start;
        float len = line.magnitude;
        line.Normalize();

        float projectLength = Mathf.Clamp(Vector3.Dot(point - start, line), 0f, len);
        return start + line * projectLength;
    }


    private void UdpateCar()
    {
        float forwardPower = Math.Clamp(Brain.Acceleration, -0.5f, 1f);
        Vector3 moveForce = EnginePower * Vector3.forward;

        var maxRotationAngle = MaxRoatationAngle * Time.deltaTime;
        var angle = Math.Clamp(Brain.AngleRotation, -maxRotationAngle, maxRotationAngle);

        transform.Rotate(0, angle, 0);
        _rb.AddRelativeForce(moveForce);
    }

    private void OnDrawGizmos()
    {
        if(_roadPoints == null || _roadPoints.Length == 0)
            return;

        for (int i = 0; i < _roadPoints.Length; i++)
        {
            var point = _roadPoints[i];
            var color = point.Achived ? Color.green : Color.red;
            Gizmos.color = color;

            Gizmos.DrawSphere(point.Position, 0.5f);
        }
    }

}