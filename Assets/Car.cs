using Mono.Cecil;
using System;
using System.Linq;
using System.Xml.Linq;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct RoadWayPoint
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

    public AnimationCurve MaxRoatationAngle;

    [Space]
    public float Speed;

    [Space]
    public CarProcessor Brain;

    public RoadWayPoint NextPoint;

    private RoadWayPoint[] _roadWayPoints;
    private Vector3[] _roadPoints;
    private int _roadResolution;

    private Rigidbody _rb;
    private Transform _trans;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _trans = GetComponent<Transform>();
    }

    public void Init(Vector3[] roadWayPoints, Vector3[] raodPoints, int roadResolution)
    {
        _roadWayPoints = new RoadWayPoint[roadWayPoints.Length];
        for (int i = 0; i < roadWayPoints.Length; i++)
        {
            _roadWayPoints[i] = new RoadWayPoint()
            {
                Ind = i,
                Achived = false,
                Position = roadWayPoints[i]
            };
        }

        _roadPoints = new Vector3[raodPoints.Length];
        raodPoints.CopyTo(_roadPoints, 0 );

        if(Brain != null)
        {
            Brain.WayPoints = new RoadWayPoint[_roadWayPoints.Length];
            _roadWayPoints.CopyTo(Brain.WayPoints, 0);
        }
    }

    public void Process()
    {
        var currPosition = _trans.position;
        var closestPoint = GetClosestRoadPoint(currPosition);

        var nextWayPoint = _roadWayPoints.First(x => x.Achived == false);
        NextPoint = nextWayPoint;

        Brain.Process(currPosition, _rb.linearVelocity, transform.forward, nextWayPoint.Ind, closestPoint);
        UdpateCar();
    }

    public void CheckAchived()
    {
        var point = _roadWayPoints[NextPoint.Ind];

        var distVect = point.Position - this.transform.position;
        distVect.y = 0;
        if (distVect.sqrMagnitude <= AchiveDistSqr)
        {
            point.Achived = true;
            _roadWayPoints[NextPoint.Ind] = point;

            int nextPointInd = NextPoint.Ind + 1;
            if(nextPointInd >= _roadWayPoints.Length)
                nextPointInd = 0;

            NextPoint = _roadWayPoints[nextPointInd];
        }
    }


    public Vector3 GetClosestRoadPoint(Vector3 currentPos)
    {
        int closestIndex = 0;
        int secondClosest = 1;
        if (_roadWayPoints == null || _roadWayPoints.Length == 0) return currentPos;

        float minDistanceSqr = float.MaxValue;

        // 1. Ищем ближайший индекс в массиве маршрутных точек
        for (int i = 0; i < _roadWayPoints.Length; i++)
        {
            float distSqr = (currentPos - _roadWayPoints[i].Position).sqrMagnitude;
            if (distSqr < minDistanceSqr)
            {
                minDistanceSqr = distSqr;
                secondClosest = closestIndex;
                closestIndex = i;
            }
        }

        // 2. Уточняем положение между сегментами за счёт детальной коллекции
        // точек дороги
        int minWayPointInd = Math.Min(closestIndex, secondClosest);
        int maxWayPointInd = Mathf.Max(closestIndex, secondClosest);

        int minRoadPointInd = minWayPointInd * _roadResolution;
        int maxRoadPointInd = maxWayPointInd * _roadResolution;

        float sqrMinDist = float.MaxValue;
        int roadPointClosestInd = 0;
        currentPos.y = 0;
        for(int i = minRoadPointInd; i < maxRoadPointInd; i++)
        {
            var pointPosition = _roadPoints[i];
            var sqrDist = (currentPos - pointPosition).sqrMagnitude;
            if(sqrDist < sqrMinDist)
            {
                sqrMinDist = sqrDist;
                roadPointClosestInd = i;
            }
        }

        return _roadPoints[roadPointClosestInd];
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
        // отображаем скорость
        Speed = _rb.linearVelocity.magnitude;

        float forwardPower = Math.Clamp(Brain.Acceleration, -0.5f, 1f);
        Vector3 moveForce = EnginePower * Vector3.forward;

        var maxRotationAngle = MaxRoatationAngle.Evaluate(Speed) * Time.deltaTime;
        var angle = Math.Clamp(Brain.AngleRotation, -maxRotationAngle, maxRotationAngle);

        transform.Rotate(0, angle, 0);
        _rb.AddRelativeForce(moveForce);


    }

    private void OnDrawGizmos()
    {
        if (UnityEditor.Selection.activeGameObject != gameObject)
        {
            return;
        }

        if (_roadWayPoints == null || _roadWayPoints.Length == 0)
            return;

        for (int i = 0; i < _roadWayPoints.Length; i++)
        {
            var point = _roadWayPoints[i];
            var color = point.Achived ? Color.green : Color.red;
            Gizmos.color = color;

            Gizmos.DrawSphere(point.Position, 0.5f);
        }
    }

}