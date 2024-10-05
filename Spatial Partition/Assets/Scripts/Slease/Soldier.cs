using SpatialPartition.Grid;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialPartitionPattern
{
    // The soldier base class for enemies and friendlies
    public class Soldier
    {
        public enum soldierType
        {
            enemy,
            friendly
        }

        public soldierType type;

        public MeshRenderer soldierMeshRenderer;
        public Transform soldierTrans;
        public float walkSpeed;

        // TOUR STOP 05
        // connect soldiers in a doubly linked list so we can easily partition them in space
        public Soldier previousSoldier;
        public Soldier nextSoldier;
        public Soldier targetSoldier;

        public virtual void Move() { }
        public virtual void Move(Soldier soldier) { }


    }


}