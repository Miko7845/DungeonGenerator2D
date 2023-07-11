using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum DungeonType { Caverns, Rooms }

public class DungeonManager : MonoBehaviour
{
    public GameObject[] randomItems, randomEnemies, roundedEdges;
    public GameObject floorPrefab, wallPrefab, tilePrefab, exitPrefab;
    [Range(50, 5000)] public int totalFloorCount;
    [Range(0, 100)] public int itemSpawnPercent;
    [Range(0, 100)] public int enemySpawnPercent;
    public bool useRoundedEdges;
    public DungeonType dungeonType;

    [HideInInspector] public float minX, maxX, minY, maxY;  // Storing the minimum and maximum coordinates of the floor.

    List<Vector3> floorList = new List<Vector3>();
    LayerMask floorMask, wallMask;
    Vector2 hitSize;

    void Start()
    {
        hitSize = Vector2.one * 0.8f;
        floorMask = LayerMask.GetMask("Floor");
        wallMask = LayerMask.GetMask("Wall");
        
        switch (dungeonType)
        {
            case DungeonType.Caverns: RandomWalker(); break;
            case DungeonType.Rooms:   RoomWalker(); break;
        }
    }

    void Update()
    {
        if(Application.isEditor && Input.GetKeyDown(KeyCode.Backspace))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    /// <summary> Dungeon generation by random walk </summary>
    void RandomWalker()
    {
        // Add a starting point to the list
        Vector3 curPos = Vector3.zero;
        floorList.Add(curPos);

        while (floorList.Count < totalFloorCount)
        {
            curPos += RandomDirection();
            if (!InFloorList(curPos)) floorList.Add(curPos);
        }
        StartCoroutine(DelayProgress());
    }

    /// <summary>  </summary>
    void RoomWalker()
    {
        // Add a starting point to the list
        Vector3 curPos = Vector3.zero;
        floorList.Add(curPos);

        while (floorList.Count < totalFloorCount)
        {
            Vector3 walkDir = RandomDirection();
            int walkLength = Random.Range(9, 18);

            curPos += RandomDirection();
            if (!InFloorList(curPos)) floorList.Add(curPos);
        }
        StartCoroutine(DelayProgress());
    }

    /// <summary> Check if the new position is free </summary>
    bool InFloorList(Vector3 myPos)
    {
        // If there is already a similar position in the list, it is forbidden to add a new position to the list.
        for (int i = 0; i < floorList.Count; i++) if (Equals(myPos, floorList[i])) return true;
        return false;
    }

    /// <summary> Choose random direction </summary>
    Vector3 RandomDirection()
    {
        switch (Random.Range(1, 5))
        {
            case 1: return Vector3.up;
            case 2: return Vector3.right;
            case 3: return Vector3.down;
            case 4: return Vector3.left;
        }
        return Vector3.zero;
    }

    /// <summary>  </summary>
    IEnumerator DelayProgress()
    {
        for (int i = 0; i < floorList.Count; i++)
        {
            GameObject goTile = Instantiate(tilePrefab, floorList[i], Quaternion.identity) as GameObject;
            goTile.name = tilePrefab.name;
            goTile.transform.SetParent(transform);
        }

        // As long as there are objects of type TileSpawner in the scene
        while (FindObjectsOfType<TileSpawner>().Length > 0)
        {
            yield return null;
        }
        ExitDoorway();

        for (int x = (int)minX - 2; x <= (int)maxX; x++)
        {
            for (int y = (int)minY - 2; y <= (int)maxY; y++)
            {
                // Check if there is a floor at this position.
                Collider2D hitFloor = Physics2D.OverlapBox(new Vector2(x, y), hitSize, 0, floorMask);
                if (hitFloor)
                {
                    // And if it's not the exit doorway.
                    if (!Equals(hitFloor.transform.position, floorList[floorList.Count - 1]))
                    {
                        // Check if there are walls on top, bottom, right and left.
                        Collider2D hitTop = Physics2D.OverlapBox(new Vector2(x, y + 1), hitSize, 0, wallMask);
                        Collider2D hitBottom = Physics2D.OverlapBox(new Vector2(x, y - 1), hitSize, 0, wallMask);
                        Collider2D hitRight = Physics2D.OverlapBox(new Vector2(x + 1, y), hitSize, 0, wallMask);
                        Collider2D hitLeft = Physics2D.OverlapBox(new Vector2(x - 1, y), hitSize, 0, wallMask);

                        // Call method to create random items.
                        RandomItems(hitFloor, hitTop, hitBottom, hitRight, hitLeft);
                        RandomEnemies(hitFloor, hitTop, hitBottom, hitRight, hitLeft);
                    }
                }
                RoundedEdges(x, y);
            }
        }
    }

    /// <summary> Rounds all walls </summary>
    void RoundedEdges(int x, int y)
    {
        if (useRoundedEdges)
        {
            Collider2D hitWall = Physics2D.OverlapBox(new Vector2(x, y), hitSize, 0, wallMask);
            if (hitWall)
            {
                Collider2D hitTop = Physics2D.OverlapBox(new Vector2(x, y + 1), hitSize, 0, wallMask);
                Collider2D hitBottom = Physics2D.OverlapBox(new Vector2(x, y - 1), hitSize, 0, wallMask);
                Collider2D hitRight = Physics2D.OverlapBox(new Vector2(x + 1, y), hitSize, 0, wallMask);
                Collider2D hitLeft = Physics2D.OverlapBox(new Vector2(x - 1, y), hitSize, 0, wallMask);
                int bitVal = 0;

                // 
                if (!hitTop) bitVal += 1;
                if (!hitRight) bitVal += 2;
                if (!hitBottom) bitVal += 4;
                if (!hitLeft) bitVal += 8;
                if (bitVal > 0)
                {
                    GameObject goEdge = Instantiate(roundedEdges[bitVal], new Vector2(x, y), Quaternion.identity) as GameObject;
                    goEdge.name = roundedEdges[bitVal].name;
                    goEdge.transform.SetParent(hitWall.transform);
                }
            }
        }
    }

    /// <summary> Create random items </summary>
    void RandomItems(Collider2D hitFloor, Collider2D hitTop, Collider2D hitBottom, Collider2D hitRight, Collider2D hitLeft)
    {
        // If the floor has at least one adjacent wall and is not angular.
        if ((hitTop || hitBottom ||  hitRight || hitLeft) && !(hitTop && hitBottom) && !(hitLeft && hitRight))
        {
            // If the number rolled is less than or equal to the percentage.
            int roll = Random.Range(1, 101);
            if (roll <= itemSpawnPercent) 
            {
                int itemIndex = Random.Range(0, randomItems.Length); // Pick a random item.
                GameObject goItem = Instantiate(randomItems[itemIndex], hitFloor.transform.position, Quaternion.identity) as GameObject; // Create an item at the floor position.
                goItem.name = randomItems[itemIndex].name;
                goItem.transform.SetParent(hitFloor.transform);
            }
        }
    }

    /// <summary> Create random enemy </summary>
    void RandomEnemies(Collider2D hitFloor, Collider2D hitTop, Collider2D hitBottom, Collider2D hitRight, Collider2D hitLeft)
    {
        // If there is no wall next to the floor.
        if (!hitTop && !hitRight &&  !hitLeft && !hitBottom)
        {
            // If the number rolled is less than or equal to the percentage.
            int roll = Random.Range(1, 101);
            if (roll <= enemySpawnPercent)
            {
                int enemyIndex = Random.Range(0, randomEnemies.Length); // Pick a random enemy.
                GameObject goEnemy = Instantiate(randomEnemies[enemyIndex], hitFloor.transform.position, Quaternion.identity) as GameObject; // Create an enemy at the floor position.
                goEnemy.name = randomEnemies[enemyIndex].name;
                goEnemy.transform.SetParent(hitFloor.transform);
            }
        }
    }

    /// <summary> Create an exit doorway </summary>
    void ExitDoorway()
    {
        Vector3 doorPos = floorList[floorList.Count - 1];
        GameObject goDoor = Instantiate(exitPrefab, doorPos, Quaternion.identity) as GameObject;
        goDoor.name = exitPrefab.name;
        goDoor.transform.SetParent(transform);
    }
}