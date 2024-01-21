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
            AddComponent<PlayerComponentData>(GetEntity(TransformUsageFlags.None)); // TransformUsageFlags.None f�r att det inte beh�ver synas i v�rlden, till skillnad fr�n player som �r Dynamic
        }
    }


}


[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct PlayerComponentData : IComponentData
{

    // Addera alla datas f�r alla statesl, g�r ett test!
    private bool testDataBool;


}

