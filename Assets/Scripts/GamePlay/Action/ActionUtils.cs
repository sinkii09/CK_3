using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public static class ActionUtils 
{
    static RaycastHit[] s_Hits = new RaycastHit[4];
    static int s_PCLayer = -1;
    static int s_NpcLayer = -1;
    static int s_EnvironmentLayer = -1;

    static readonly Vector3 k_CharacterEyelineOffset = new Vector3(0, 1, 0);

    const float k_CloseDistanceOffset = 1;

    const float k_VeryCloseTeleportRange = k_CloseDistanceOffset + 1;
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
    public static bool IsValidTarget(ulong targetId)
    {
        //note that we DON'T check if you're an ally. It's perfectly valid to target friends,
        //because there are friendly skills, such as Heal.

        if (NetworkManager.Singleton.SpawnManager == null || !NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out var targetChar))
        {
            return false;
        }

        var targetable = targetChar.GetComponent<ITargetable>();
        return targetable != null && targetable.IsValidTarget;
    }
    public static Vector3 GetDashDestination(Transform characterTransform, Vector3 targetSpot, bool stopAtObstructions, float distanceToUseIfVeryClose = -1, float maxDistance = -1)
    {
        Vector3 destinationSpot = targetSpot;

        if (distanceToUseIfVeryClose != -1)
        {
            // make sure our stopping point is a meaningful distance away!
            if (destinationSpot == Vector3.zero || Vector3.Distance(characterTransform.position, destinationSpot) <= k_VeryCloseTeleportRange)
            {
                // we don't have a meaningful stopping spot. Find a new one based on the character's current direction
                destinationSpot = characterTransform.position + characterTransform.forward * distanceToUseIfVeryClose;
            }
        }

        if (maxDistance != -1)
        {
            // make sure our stopping point isn't too far away!
            float distance = Vector3.Distance(characterTransform.position, destinationSpot);
            if (distance > maxDistance)
            {
                destinationSpot = Vector3.MoveTowards(destinationSpot, characterTransform.position, distance - maxDistance);
            }
        }

        if (stopAtObstructions)
        {
            // if we're going to hit an obstruction, stop at the obstruction
            if (!HasLineOfSight(characterTransform.position, destinationSpot, out Vector3 collidePos))
            {
                destinationSpot = collidePos;
            }
        }

        // now get a spot "near" the end point
        destinationSpot = Vector3.MoveTowards(destinationSpot, characterTransform.position, k_CloseDistanceOffset);

        return destinationSpot;
    }
    public static bool HasLineOfSight(Vector3 character1Pos, Vector3 character2Pos, out Vector3 missPos)
    {
        if (s_EnvironmentLayer == -1)
        {
            s_EnvironmentLayer = LayerMask.NameToLayer("Environment");
        }

        int mask = 1 << s_EnvironmentLayer;

        character1Pos += k_CharacterEyelineOffset;
        character2Pos += k_CharacterEyelineOffset;
        var rayDirection = character2Pos - character1Pos;
        var distance = rayDirection.magnitude;

        var numHits = Physics.RaycastNonAlloc(new Ray(character1Pos, rayDirection), s_Hits, distance, mask);
        if (numHits == 0)
        {
            missPos = character2Pos;
            return true;
        }
        else
        {
            missPos = s_Hits[0].point;
            return false;
        }
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
