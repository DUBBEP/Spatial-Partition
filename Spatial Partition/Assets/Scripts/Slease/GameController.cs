using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SpatialPartitionPattern
{
    public class GameController : MonoBehaviour
    {
        // depends on: Soldier, Grid, Enemy, Friendly

        public GameObject friendlyObj;
        public GameObject enemyObj;
        public TextMeshProUGUI UpdateTimeText;
        public TextMeshProUGUI usingPatialPartition;
        public TextMeshProUGUI soldierlimitText;

        public Material enemyMaterial;
        public Material closestEnemyMaterial;

        public Transform enemyParent;
        public Transform friendlyParent;

        List<Soldier> enemySoldiers = new List<Soldier>();
        List<Soldier> friendlySoldiers = new List<Soldier>();

        List<Soldier> closestEnemies = new List<Soldier>();

        public float shatterForce;
        public float torqueForce;

        private int soldiersOnBoard;
        public int SoldiersOnBoard {  get { return soldiersOnBoard; } }

        [SerializeField]
        private int soldierLimit;
        public int SoldierLimit {  get { return soldierLimit; } }

        // Grid data
        float mapWidth = 50f;
        int cellSize = 10;

        // Number of soldiers on each team


        public int soldiersToSpawn;
        public bool useSpatialParition = true;

        // The spatial partition grid
        Grid grid;

        public static GameController instance;

        void Awake() { instance = this; }

        // Start is called before the first frame update
        void Start()
        {
            grid = new Grid((int)mapWidth, cellSize);
            ToggleSpacialPartition();
            UpdateSoldierLimitText();
        }

        public void UpdateSoldierLimitText()
        {
            soldierlimitText.text = "Soldier Limit: " + soldierLimit;
        }

        public void ChangeSoldierLimit(int count)
        {
            soldierLimit += count;
            UpdateSoldierLimitText();
        }

        public void ResetScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void SpawnSoldiers()
        {
            soldiersOnBoard = 2;

            // add random enemies and friendlies and store them in a list
            Vector3 randomPos = new Vector3(Random.Range(0f, mapWidth), 0.5f, Random.Range(0f, mapWidth));
            GameObject newEnemy = Instantiate(enemyObj, randomPos, Quaternion.identity) as GameObject;
            enemySoldiers.Add(new Enemy(newEnemy, mapWidth, grid));
            newEnemy.transform.parent = enemyParent;

            randomPos = new Vector3(Random.Range(0f, mapWidth), 0.5f, Random.Range(0f, mapWidth));
            GameObject newFriendly = Instantiate(friendlyObj, randomPos, Quaternion.identity) as GameObject;
            friendlySoldiers.Add(new Friendly(newFriendly, mapWidth));
            newFriendly.transform.parent = friendlyParent;
        }

        // Update is called once per frame
        void Update()
        {
            // TOUR STOP 01 - measuring performance, easy mode
            float startTime = Time.realtimeSinceStartup;

            // move the enemies
            for (int i = 0; i < enemySoldiers.Count; i++)
            {
                enemySoldiers[i].Move();
            }

            // reset material of the closest enemies
            for (int i = 0; i < closestEnemies.Count; i++)
            {
                closestEnemies[i].soldierMeshRenderer.material = enemyMaterial;
            }

            // reset the list with the closest enemies
            closestEnemies.Clear();
            Soldier closestEnemy;
            for (int i = 0; i < friendlySoldiers.Count; i++)
            {
                // TOUR STOP 02 - toggle spatial partition optimization
                if (useSpatialParition)
                {
                    closestEnemy = grid.FindClosestEnemy(friendlySoldiers[i]);
                }
                else
                {
                    closestEnemy = FindClosestEnemySlow(friendlySoldiers[i]);
                }
                if (closestEnemy != null)
                {
                    closestEnemy.soldierMeshRenderer.material = closestEnemyMaterial;
                    closestEnemies.Add(closestEnemy);
                    friendlySoldiers[i].Move(closestEnemy);
                }
            }

            float elapsedTime = (Time.realtimeSinceStartup - startTime) * 1000f;
            elapsedTime = Mathf.Round(elapsedTime * 1000.0f) / 1000.0f;
            UpdateTimeText.text = "Update Time: " + elapsedTime.ToString() + "ms";
            // Debug.Log(elapsedTime + "ms");
        }

        // TOUR STOP 03 - Find the closest enemy, slow version
        Soldier FindClosestEnemySlow(Soldier soldier)
        {
            Soldier closestEnemy = null;
            float bestDistSqr = Mathf.Infinity;

            // loop through all enemies
            for (int i = 0; i < enemySoldiers.Count; i++)
            {
                float distSqr = (soldier.soldierTrans.position - enemySoldiers[i].soldierTrans.position).sqrMagnitude;
                if (distSqr < bestDistSqr)
                {
                    bestDistSqr = distSqr;
                    closestEnemy = enemySoldiers[i];
                }
            }

            return closestEnemy;
        }

        public void ToggleSpacialPartition()
        {
            if (useSpatialParition)
                useSpatialParition = false;
            else
                useSpatialParition = true;

            usingPatialPartition.text = "Using Spacial partition: " + useSpatialParition.ToString();
        }

        public void BreakApart(int numOfPieces, CollisionBehavior soldierObject)
        {
            if (soldiersOnBoard >= soldierLimit)
                return;

            soldiersOnBoard += numOfPieces;
            // spawn three smaller enemy soldiers, send them flying in a random direction and destroy self
            for (int i = 0; i < numOfPieces; ++i)
            {
                // instantiate object
                GameObject soldierShard = SpawnSoldierFragment(soldierObject);
                Rigidbody shardRb = soldierShard.GetComponent<Rigidbody>();

                Debug.Log(new Vector3(Random.Range(-1f, 1f), Random.Range(0.5f, 0.7f), Random.Range(-1f, 1f)));
                shardRb.AddForce(new Vector3(Random.Range(-1f, 1f), Random.Range(0.5f, 0.7f), Random.Range(-1f, 1f)) * shatterForce, ForceMode.Impulse);
                shardRb.AddTorque(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * torqueForce, ForceMode.Impulse);
            }
        }

        GameObject SpawnSoldierFragment(CollisionBehavior soldierObject)
        {
            if (soldierObject.type == Soldier.soldierType.enemy)
            {
                GameObject newEnemy = Instantiate(enemyObj, new Vector3(soldierObject.transform.position.x, soldierObject.transform.position.y + 0.5f, soldierObject.transform.position.z), Quaternion.identity) as GameObject;
                enemySoldiers.Add(new Enemy(newEnemy, mapWidth, grid));
                newEnemy.transform.parent = enemyParent;
                return newEnemy;
            }
            else if (soldierObject.type == Soldier.soldierType.friendly)
            {
                GameObject newFriendly = Instantiate(friendlyObj, new Vector3(soldierObject.transform.position.x, soldierObject.transform.position.y + 0.5f, soldierObject.transform.position.z), Quaternion.identity) as GameObject;
                friendlySoldiers.Add(new Friendly(newFriendly, mapWidth));
                newFriendly.transform.parent = friendlyParent;
                return newFriendly;
            }
            return null;
        }

        public void RemoveSoldier(Transform soldierTransform, Soldier.soldierType type)
        {
            if (type == Soldier.soldierType.enemy)
            {
                Soldier soldier = GetSoldierInList(soldierTransform, enemySoldiers);

                if (soldier == null)
                    return;

                enemySoldiers.Remove(soldier);
                if (closestEnemies.Contains(soldier))
                    closestEnemies.Remove(soldier);

                grid.Remove(soldier);
            }
            else if (type == Soldier.soldierType.friendly)
            {
                Soldier soldier = GetSoldierInList(soldierTransform, friendlySoldiers);
                friendlySoldiers.Remove(soldier);
                grid.Remove(soldier);
            }

            soldierTransform.gameObject.SetActive(false);
        }

        Soldier GetSoldierInList(Transform soldierTransform, List<Soldier> list)
        {
            foreach (Soldier x in list)
                if (x.soldierTrans == soldierTransform)
                    return x;

            return null;
        }
    }

}