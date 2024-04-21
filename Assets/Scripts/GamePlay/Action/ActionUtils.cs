using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ActionUtils 
{

}
public class RaycastHitComparer : IComparer<RaycastHit>
{
    public int Compare(RaycastHit x, RaycastHit y)
    {
        return x.distance.CompareTo(y.distance);
    }
}
