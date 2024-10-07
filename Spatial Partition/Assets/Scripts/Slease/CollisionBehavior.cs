using System.Collections;
using UnityEngine;

namespace SpatialPartitionPattern
{
    public class CollisionBehavior : MonoBehaviour
    {
        [SerializeField]
        private int _shatterChunks;

        bool poppable;

        public Soldier.soldierType type;
        public Enemy enemy;
        public Friendly friendly;
        private Collider col;

        private void Start()
        {
            col = GetComponent<Collider>();
            poppable = false;
            StartCoroutine(SpawnIntangibility());

            if (friendly != null)
                StartCoroutine(PauseFriendlyMove());
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Debug.Log("Collision Detected");
            if ((collision.gameObject.CompareTag("Friendly") || collision.gameObject.CompareTag("Enemy")) && poppable)
            {
                if (GameController.instance.SoldiersOnBoard >= GameController.instance.SoldierLimit)
                {
                    col.excludeLayers = LayerMask.GetMask("Friendly", "Enemy");
                    
                    if (friendly != null)
                        friendly.walkSpeed = 2f;
                    
                    poppable = false;
                    return;
                }

                GameController.instance.BreakApart(_shatterChunks, this);
                GameController.instance.RemoveSoldier(this.transform, type);
                poppable = true;
            }

            if (collision.gameObject.CompareTag("Ground"))
            {
                transform.rotation = Quaternion.identity;
                
                if (type == Soldier.soldierType.enemy)
                    enemy.GetNewTarget();
            }

            if (collision.gameObject.CompareTag("Wall"))
            {
                if (type == Soldier.soldierType.enemy)
                    enemy.GetNewTarget();
            }
        }

        IEnumerator PauseFriendlyMove()
        {
            friendly.pauseMove = true;
            yield return new WaitForSecondsRealtime(2.5f);
            friendly.pauseMove = false;
        }

        IEnumerator SpawnIntangibility()
        {
            yield return new WaitForSecondsRealtime(1);
            poppable = true;
        }
    }
}


