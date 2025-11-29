using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Route : MonoBehaviour
{
    [SerializeField] private List<Transform> routePoints = new List<Transform>();

    public int RoutePointsCount
    {
        get
        {
            //Так сделано, чтобы маршрут с 1 точкой или без точек не использовался
            if (routePoints.Count > 2)
            {
                return routePoints.Count;
            }
            else
            {
                return 0;
            }
            
        }
    }

    public bool IsEnable
    {
        get
        {
            return (routePoints.Count > 2) ? true : false;
        }
    }

    public void AddPoint(Transform point)
    {
        routePoints.Add(point);
    }

    public void Clear()
    {
        routePoints.Clear();
    }

    public void CopyRought(Route other)
    {
        routePoints = null;
        routePoints = new List<Transform>(other.routePoints.Count);

        for (int i = 0; i < other.routePoints.Count; i++)
        {
            routePoints.Add(other.routePoints[i]);
        }
    }

    public void CopyRought(List<Transform> other)
    {
        routePoints = null;
        routePoints = new List<Transform>(other.Count);

        for (int i = 0; i < other.Count; i++)
        {
            routePoints.Add(other[i]);
        }
    }

    public Transform GetPointByIndex(int index)
    {
        //Так сделано, чтобы маршрут с 1 точкой или без точек не использовался
        if (routePoints.Count > 2)
        {
            if (index > -1 && index < routePoints.Count)
            {
                return routePoints[index];
            }
        }
        return null;
    }

    public void RemovePointAt(int index)
    {
        if (index > 0 && index < routePoints.Count)
        {
            routePoints.RemoveAt(index);
        }
    }

}
