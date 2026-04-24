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

    public CarSettings Settings;

    [Space]
    public float Speed;

    [Space]
    public CarProcessor Brain;

    public RoadWayPoint NextPoint;

    private RoadWayPoint[] _roadWayPoints;
    private Vector3[] _roadPoints;
    private int _roadResolution;
    float _roadWidth;

    private Rigidbody _rb;
    private Transform _trans;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _trans = GetComponent<Transform>();
    }

    public void Init(
        Vector3[] roadWayPoints, 
        Vector3[] raodPoints, 
        int roadResolution,
        float roadWidth)
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
        raodPoints.CopyTo(_roadPoints, 0);
        _roadWidth = roadWidth;
        _roadResolution = roadResolution;

        if (Brain != null)
        {
            Brain.WayPoints = new RoadWayPoint[_roadWayPoints.Length];
            _roadWayPoints.CopyTo(Brain.WayPoints, 0);

            Brain.RoadWidth = roadWidth;
        }
    }

    public void Process()
    {
        var currPosition = _trans.position;
        var closestPoint = GetClosestRoadPoint(currPosition);

        var nextWayPoint = _roadWayPoints.FirstOrDefault(x => x.Achived == false);
        NextPoint = nextWayPoint;

        // передаём мозгу актуальные параметры
        UpdateBrainParameters();

        Brain.Process(currPosition, _rb.linearVelocity, transform.forward, nextWayPoint.Ind, closestPoint);
        UdpateCar(closestPoint);
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

            // если завершили круг - сбрасываем достижения точек
            if(nextPointInd >= _roadWayPoints.Length)
            {
                nextPointInd = 0;
                for(int i = 0; i < _roadWayPoints.Length; i++)
                {
                    var pointToReset = _roadWayPoints[i];
                    pointToReset.Achived = false;
                }
            }

            NextPoint = _roadWayPoints[nextPointInd];
        }
    }


    public Vector3 GetClosestRoadPoint(Vector3 currentPos)
    {
        int closestIndex = 0;
        if (_roadWayPoints == null || _roadWayPoints.Length == 0) return currentPos;

        float minDistanceSqr = float.MaxValue;

        // 1. Ищем ближайший индекс в массиве маршрутных точек
        for (int i = 0; i < _roadWayPoints.Length; i++)
        {
            float distSqr = (currentPos - _roadWayPoints[i].Position).sqrMagnitude;
            
            if (distSqr < minDistanceSqr)
            {
                minDistanceSqr = distSqr;
                closestIndex = i;
            }
        }

        // 2. Уточняем положение между сегментами за счёт детальной коллекции
        // точек дороги

        float closestDist = float.MaxValue;
        Vector3 closestPoint = Vector3.zero;

        CheckRoadPart(closestIndex - 1, currentPos, ref closestDist, ref closestPoint);
        CheckRoadPart(closestIndex, currentPos, ref closestDist, ref closestPoint);

        return closestPoint;
    }


    private void CheckRoadPart(int startPointInd, Vector3 currentPos, ref float closestDist, ref Vector3 closestPoint)
    {
        if (startPointInd < 0)
            startPointInd += _roadWayPoints.Length;

        if(startPointInd >=  _roadWayPoints.Length)
            startPointInd -= _roadWayPoints.Length;

        int roadPointClosestInd = -1;
        currentPos.y = 0;

        int startInd = startPointInd * _roadResolution;

        for (int i = startInd; i < startInd + _roadResolution; i++)
        {
            var pointPosition = _roadPoints[i];
            var sqrDist = (currentPos - pointPosition).sqrMagnitude;
            if (sqrDist < closestDist)
            {
                closestDist = sqrDist;
                roadPointClosestInd = i;
            }
        }

        if(roadPointClosestInd >= 0)
            closestPoint = _roadPoints[roadPointClosestInd];
    }

    private void UpdateBrainParameters()
    {
        var speed = _rb.linearVelocity.magnitude;
        var maxRotationAngle = Settings.MaxRoatationAngle.Evaluate(Speed) * Time.deltaTime;
    
        Brain.MaxRotationAngle = maxRotationAngle;
    }

    private void UdpateCar(Vector3 closestPoint)
    {
        var currPosition = transform.position;
        currPosition.y = 0f;
        closestPoint.y = 0f;

        var dist = (currPosition - closestPoint).magnitude;
        dist -= _roadWidth / 2;
        dist = Math.Max(dist, 0f);

        float forwAngle = Vector3.SignedAngle(transform.forward.normalized, _rb.linearVelocity.normalized, Vector3.up);
        float absForwAngle = math.abs(forwAngle);

        var damping = Settings.CarDampingByAngle.Evaluate(absForwAngle);
        damping *= Settings.RoadOffsetLinerDamping.Evaluate(dist);
        _rb.linearDamping = damping;

        // отображаем скорость
        Speed = _rb.linearVelocity.magnitude;

        float forwardPower = Math.Clamp(Brain.Acceleration, -0.5f, 1f);
        Vector3 moveForce = Settings.EnginePower * forwardPower * Vector3.forward;

        var maxRotationAngle = Settings.MaxRoatationAngle.Evaluate(Speed) * Time.deltaTime;
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