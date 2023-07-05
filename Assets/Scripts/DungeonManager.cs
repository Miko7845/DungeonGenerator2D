using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonManager : MonoBehaviour
{
    public GameObject[] randomItems;
    public GameObject floorPrefab, wallPrefab, tilePrefab, exitPrefab;
    [Range(50, 5000)] public int totalFloorCount;
    [Range(0, 100)] public int itemSpawnPercent;

    [HideInInspector] public float minX, maxX, minY, maxY;  // Storing the minimum and maximum coordinates of the floor.

    List<Vector3> floorList = new List<Vector3>();
    LayerMask floorMask, wallMask;

    void Start()
    {
        floorMask = LayerMask.GetMask("Floor");
        wallMask = LayerMask.GetMask("Wall");
        RandomWalker();
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
            switch (Random.Range(1, 5))
            {
                case 1: curPos += Vector3.up; break;
                case 2: curPos += Vector3.right; break;
                case 3: curPos += Vector3.down; break;
                case 4: curPos += Vector3.left; break;
            }

            // The loop checks if the new position is free. If there is already a similar
            // position in the list, it is forbidden to add a new position to the list.
            bool inFloorList = false;
            for (int i = 0; i < floorList.Count; i++)
            {
                if (Vector3.Equals(curPos, floorList[i]))
                {
                    inFloorList = true;
                    break;
                }
            }
            if (!inFloorList)
            {
                floorList.Add(curPos);
            }
        }

        for(int i = 0; i < floorList.Count; i++)
        {
            GameObject goTile = Instantiate(tilePrefab, floorList[i], Quaternion.identity) as GameObject;
            goTile.name = tilePrefab.name;
            goTile.transform.SetParent(transform);
        }
        StartCoroutine(DelayProgress());
    }

    /// <summary> Coroutine to delay progress </summary>
    IEnumerator DelayProgress()
    {
        // As long as there are objects of type TileSpawner in the scene
        while (FindObjectsOfType<TileSpawner>().Length > 0)
        {
            yield return null;
        }
        ExitDoorway();
        Vector2 hitSize = Vector2.one * 0.8f;
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
                    }
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
            int roll = Random.Range(0, 101);
            if (roll <= itemSpawnPercent) 
            {
                int itemIndex = Random.Range(0, randomItems.Length); // Pick a random item.
                GameObject goItem = Instantiate(randomItems[itemIndex], hitFloor.transform.position, Quaternion.identity) as GameObject; // Create an item at the floor position.
                goItem.name = randomItems[itemIndex].name;
                goItem.transform.SetParent(hitFloor.transform);
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