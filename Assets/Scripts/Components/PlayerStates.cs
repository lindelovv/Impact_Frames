using System.Collections;
using Unity.Entities;
using Unity.NetCode;
using System.Collections.Generic;
using UnityEngine;
using System.ComponentModel;
using Unity.Mathematics;

public class PlayerStates : MonoBehaviour
{
    [SerializeField] private bool testDataBool;

    private class Baker : Baker<PlayerStates>
    {
        public override void Bake(PlayerStates authoring)
        {
            AddComponent<PlayerComponentData>(GetEntity(TransformUsageFlags.None)); // TransformUsageFlags.None för att det inte behöver synas i världen, till skillnad från player som är Dynamic
        }
    }


}


[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct PlayerComponentData : IComponentData
{

    // Addera alla datas för alla statesl, gör ett test!
    private bool testDataBool;


}

