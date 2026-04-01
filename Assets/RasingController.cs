using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class RasingController : MonoBehaviour
{
    public SmoothRoadGenerator Road;

    public List<Car> Cars;

    private void Awake()
    {
        Vector3[] points = new Vector3[Road.waypoints.Count];
        int ind = 0;
        foreach(var wPoint  in Road.waypoints)
        {
            points[ind] = wPoint.position;
            ind++;
        }    

        foreach (Car car in Cars)
        {
            car.Init(points);
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