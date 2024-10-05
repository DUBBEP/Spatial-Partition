using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialPartitionPattern
{
    public class Friendly : Soldier
    {
        public bool pauseMove;
        public Friendly(GameObject soldierObj, float mapWidth)
        {
            this.soldierTrans = soldierObj.transform;
            this.walkSpeed = 4f;
            this.type = soldierType.friendly;
            soldierObj.GetComponent<CollisionBehavior>().friendly = this;
        }

        // mvoe toward the closest enemy - will always move within its grid
        public override void Move(Soldier closestEnemy)
        {
            if (pauseMove)
                return;

            soldierTrans.rotation = Quaternion.LookRotation(closestEnemy.soldierTrans.position - soldierTrans.position);
            soldierTrans.Translate(Vector3.forward * Time.deltaTime * walkSpeed);
        }
    }
}