using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks; // Import UniTask

public class Maze : MonoBehaviour
{
    public int width = 11;
    public int height = 11;
    public GameObject wallPrefab;
    public GameObject finishPrefab;
    public GameObject finishObject;
    public GameObject playerPrefab;
    public Transform mazeParent;

    private int[,] maze;
    private Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
    private Vector2Int playerSpawn;
    private Vector2Int finishSpawn;

    void Start()
    {
        GenerateMaze();
        DrawMaze();
        SpawnPlayerAndFinish();
    }

    void GenerateMaze()
    {
        maze = new int[width, height];

       
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                maze[x, y] = 0;
            }
        }

        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        Vector2Int start = new Vector2Int(1, 1);
        maze[start.x, start.y] = 1;
        stack.Push(start);

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek();
            List<Vector2Int> neighbors = GetUnvisitedNeighbors(current);

            if (neighbors.Count > 0)
            {
                Vector2Int chosen = neighbors[Random.Range(0, neighbors.Count)];
                Vector2Int between = (current + chosen) / 2;
                maze[between.x, between.y] = 1; 
                maze[chosen.x, chosen.y] = 1;
                stack.Push(chosen);
            }
            else
            {
                stack.Pop();
            }
        }


        playerSpawn = start;
        finishSpawn = GetFurthestPointFromStart();
        maze[finishSpawn.x, finishSpawn.y] = 3; 
    }

    List<Vector2Int> GetUnvisitedNeighbors(Vector2Int cell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighbor = cell + dir * 2;
            if (neighbor.x > 0 && neighbor.x < width - 1 && neighbor.y > 0 && neighbor.y < height - 1 && maze[neighbor.x, neighbor.y] == 0)
            {
                neighbors.Add(neighbor);
            }
        }
        return neighbors;
    }

    Vector2Int GetFurthestPointFromStart()
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> distance = new Dictionary<Vector2Int, int>();
        Vector2Int furthestPoint = playerSpawn;
        int maxDist = 0;

        queue.Enqueue(playerSpawn);
        distance[playerSpawn] = 0;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbor = current + dir;
                if (neighbor.x > 0 && neighbor.x < width - 1 && neighbor.y > 0 && neighbor.y < height - 1)
                {
                    if (maze[neighbor.x, neighbor.y] == 1 && !distance.ContainsKey(neighbor))
                    {
                        distance[neighbor] = distance[current] + 1;
                        queue.Enqueue(neighbor);

                        if (distance[neighbor] > maxDist)
                        {
                            maxDist = distance[neighbor];
                            furthestPoint = neighbor;
                        }
                    }
                }
            }
        }
        return furthestPoint;
    }

    void DrawMaze()
    {
        Vector3 finishPos = Vector3.zero;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x, 0, y);
                if (maze[x, y] == 0)
                {
                    GameObject obj = Instantiate(wallPrefab, pos, Quaternion.identity);
                    obj.transform.SetParent(mazeParent);
                }
                else if (maze[x, y] == 3)
                {
                    GameObject obj = Instantiate(finishPrefab, pos, Quaternion.identity);
                    obj.transform.SetParent(mazeParent);
                    finishPos = pos;
                }
            }
        }
        //set position
        mazeParent.transform.position = new Vector3(-7.73f, -13.93f, 37.04f);
    }

    void SpawnPlayerAndFinish()
    {
        Vector3 startPosition = new Vector3(playerSpawn.x, 0.5f, playerSpawn.y);
        Vector3 finishPosition = new Vector3(finishSpawn.x, 0.5f, finishSpawn.y);

        Instantiate(finishObject, finishPosition, Quaternion.identity);

        Debug.Log($"Player Spawned at: {playerSpawn}");
        Debug.Log($"Finish Spawned at: {finishSpawn}");

    }
}
