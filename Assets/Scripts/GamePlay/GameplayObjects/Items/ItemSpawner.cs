using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ItemSpawner : NetworkBehaviour
{
    LayerMask k_EvenvironmentLayerMask;
    LayerMask k_ItemLayerMask;
    [SerializeField] private List<NetworkObject> m_ItemList;
    [SerializeField] private float m_TimeBetweenSpawn = 1f;
    [SerializeField] private float m_DelayStartTime = 30f;
    [SerializeField] private float m_SpawnRange = 10f;
    bool m_IsStarted;

    int m_WaveIndex;

    List<NetworkObject> m_AtiveSpawns = new List<NetworkObject>();

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            enabled = false;
            return;
        }
        m_IsStarted = true;

        k_EvenvironmentLayerMask = LayerMask.GetMask(new[] { "Environment" });
        k_ItemLayerMask = LayerMask.GetMask(new[] { "Item" });
        Debug.Log(m_ItemList.Count);
        StartCoroutine(SpawnWaves());
    }
    IEnumerator SpawnWaves()
    {
        m_WaveIndex = 0;
        yield return null;
        while(true)
        {
            yield return SpawnWave();
            yield return new WaitForSeconds(10);
        }  
    }
    IEnumerator SpawnWave()
    {
        Debug.Log(PlayerServerCharacter.GetPlayerServerCharacters().Count > 0);
        foreach (var player in PlayerServerCharacter.GetPlayerServerCharacters())
        {
            var newSpawn = SpawnItem();
            m_AtiveSpawns.Add(newSpawn);
            yield return new WaitForSeconds(3f);
        }
    }
    NetworkObject SpawnItem()
    {
        int idx = Random.Range(0, m_ItemList.Count);
        Debug.Log(idx);
        var clone = Instantiate(m_ItemList[idx],RandomPositionInRange(),Quaternion.identity);
        if(!clone.IsSpawned)
        {
            clone.Spawn();
        }
        return clone;
    }
    Vector3 RandomPositionInRange()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 newPosition = transform.position + new Vector3(Mathf.Cos(angle)*m_SpawnRange,1,Mathf.Sin(angle)*m_SpawnRange);
        if(!IsAvailablePosion(newPosition))
        {
            RandomPositionInRange();
        }
        return newPosition;
    }
    bool IsAvailablePosion(Vector3 position)
    {
        if(Physics.CheckSphere(position,5f,k_ItemLayerMask))
        {
            return false;
        }
        if(Physics.Raycast(position, Vector3.up, 100f, k_EvenvironmentLayerMask))
        {
            return false;
        }
        return true;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position,m_SpawnRange);
    }
}
