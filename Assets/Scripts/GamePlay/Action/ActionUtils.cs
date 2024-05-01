using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ActionUtils 
{
    static RaycastHit[] s_Hits = new RaycastHit[4];
    static int s_PCLayer = -1;
    static int s_NpcLayer = -1;
    static int s_EnvironmentLayer = -1;
    public static int DetectMeleeFoe(bool isNPC, Collider attacker, float range, out RaycastHit[] results)
    {
        return DetectNearbyEntities(isNPC, !isNPC, attacker, range, out results);
    }
    public static int DetectNearbyEntities(bool wantPcs, bool wantNpcs, Collider attacker, float range, out RaycastHit[] results)
    {
        //this simple detect just does a boxcast out from our position in the direction we're facing, out to the range of the attack.

        var myBounds = attacker.bounds;

        if (s_PCLayer == -1)
            s_PCLayer = LayerMask.NameToLayer("PCs");
        if (s_NpcLayer == -1)
            s_NpcLayer = LayerMask.NameToLayer("NPCs");

        int mask = 0;
        if (wantPcs)
            mask |= (1 << s_PCLayer);
        if (wantNpcs)
            mask |= (1 << s_NpcLayer);

        int numResults = Physics.BoxCastNonAlloc(attacker.transform.position, myBounds.extents,
            attacker.transform.forward, s_Hits, Quaternion.identity, range, mask);

        results = s_Hits;
        return numResults;
    }
    public static float GetPercentChargedUp(float stoppedChargingUpTime, float timeRunning, float timeStarted, float execTime)
    {
        float timeSpentChargingUp;
        if (stoppedChargingUpTime == 0)
        {
            timeSpentChargingUp = timeRunning; 
        }
        else
        {
            timeSpentChargingUp = stoppedChargingUpTime - timeStarted;
        }
        return Mathf.Clamp01(timeSpentChargingUp / execTime);
    }
}

public static class ActionConclusion
{
    public const bool Stop = false;
    public const bool Continue = true;
}

public class RaycastHitComparer : IComparer<RaycastHit>
{
    public int Compare(RaycastHit x, RaycastHit y)
    {
        return x.distance.CompareTo(y.distance);
    }
}
