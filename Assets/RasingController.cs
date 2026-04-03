using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class RasingController : MonoBehaviour
{
    public SmoothRoadGenerator Road;

    public List<Car> Cars;

    private void Awake()
    {
        Vector3[] wayPoints = new Vector3[Road.waypoints.Count];
        int ind = 0;
        foreach(var wPoint  in Road.waypoints)
        {
            wayPoints[ind] = wPoint.position;
            ind++;
        }    

        foreach (Car car in Cars)
        {
            car.Init(wayPoints, Road.generatedPoints, Road.resolution, Road.roadWidth);
        }
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < Cars.Count; i++)
        {
            var car = Cars[i];
            car.Process();
            car.CheckAchived();
        }
    }
}