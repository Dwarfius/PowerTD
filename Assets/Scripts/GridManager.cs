using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TerrainGenerator))]
public class GridManager : MonoBehaviour
{
    public int gridSize = 100;
    public float cellSize = 1;

    public static GridManager Instance { get; private set; }

    GameObject[,] grid;
    Rect worldEdges;
    TerrainGenerator terrainGen;

    void Awake ()
    {
        Instance = this;
        Pathfinding.Instance.Init(this);

        grid = new GameObject[gridSize, gridSize];
        worldEdges = new Rect(new Vector2(-gridSize/2, -gridSize/2) * cellSize, new Vector2(gridSize, gridSize) * cellSize);

        terrainGen = GetComponent<TerrainGenerator>();
        terrainGen.Generate(gridSize);
	}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            terrainGen.Generate(gridSize);
    }

    public bool showCells;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector2 botLeft = -new Vector2(gridSize / 2f * cellSize, gridSize / 2f * cellSize);
        Vector2 topRight = -botLeft;
        Vector2 topLeft = new Vector2(botLeft.x, topRight.y);
        Vector2 botRight = new Vector2(topRight.x, botLeft.y);

        Gizmos.DrawLine(botLeft, topLeft);
        Gizmos.DrawLine(botLeft, botRight);
        Gizmos.DrawLine(botRight, topRight);
        Gizmos.DrawLine(topLeft, topRight);

        if(showCells)
        {
            Gizmos.color = Color.blue;
            for (float x = botLeft.x + cellSize; x < botRight.x; x += cellSize)
                Gizmos.DrawLine(new Vector2(x, botLeft.y), new Vector2(x, topLeft.y));
            for (float y = botLeft.y + cellSize; y < topLeft.y; y += cellSize)
                Gizmos.DrawLine(new Vector2(botLeft.x, y), new Vector2(botRight.x, y));
        }
    }

    public Vector2 GetIndices(Vector2 pos)
    {
        Vector2 offset = new Vector2(gridSize / 2 * cellSize, gridSize / 2 * cellSize);
        Vector2 scaledDown = pos / cellSize;
        Vector2 offsetPos = scaledDown + offset;
        return new Vector2(Mathf.Floor(offsetPos.x), Mathf.Floor(offsetPos.y));
    }

    public void Add(GameObject go)
    {
        Vector2 indices = GetIndices(go.transform.position);
        grid[(int)indices.x, (int)indices.y] = go;

        // this is optional, since BuildManager already handles positioning
        // this is left here for those buildings which are already pre-placed in scene
        Vector2 offset = new Vector2(gridSize / 2 * cellSize, gridSize / 2 * cellSize);
        Vector2 cellCenter = indices * cellSize - offset + new Vector2(cellSize / 2, cellSize / 2);
        go.transform.position = cellCenter;

        // notify neighbors - again, only 4 dirs
        GameObject neighbor = GetAtIndices(indices + Vector2.up);
        if (neighbor)
            neighbor.GetComponent<Building>().NotifyNeighborPlaced();

        neighbor = GetAtIndices(indices + Vector2.down);
        if (neighbor)
            neighbor.GetComponent<Building>().NotifyNeighborPlaced();

        neighbor = GetAtIndices(indices + Vector2.right);
        if (neighbor)
            neighbor.GetComponent<Building>().NotifyNeighborPlaced();

        neighbor = GetAtIndices(indices + Vector2.left);
        if (neighbor)
            neighbor.GetComponent<Building>().NotifyNeighborPlaced();
    }

    public void Remove(GameObject go)
    {
        Vector2 gridIndex = GetIndices(go.transform.position);
        grid[(int)gridIndex.x, (int)gridIndex.y] = null;
    }

    // returns the center of the node that contains pos in world space
    public Vector2 GetCenter(Vector2 pos)
    {
        Vector2 gridIndex = GetIndices(pos);
        Vector2 offset = new Vector2(gridSize / 2 * cellSize, gridSize / 2 * cellSize);
        return gridIndex * cellSize - offset + new Vector2(cellSize / 2, cellSize / 2);
    }

    // returns the center of the node (world space) at indices
    public Vector2 GetCenterAtIndex(int x, int y)
    {
        Vector2 offset = new Vector2(gridSize / 2 * cellSize, gridSize / 2 * cellSize);
        return new Vector2(x, y) * cellSize - offset + new Vector2(cellSize / 2, cellSize / 2);
    }

    public bool IsFree(Vector2 pos)
    {
        Vector2 indices = GetIndices(pos);
        return grid[(int)indices.x, (int)indices.y] == null;
    }

    public GameObject GetAtIndices(Vector2 indices)
    {
        return grid[(int)indices.x, (int)indices.y];
    }

    public Rect GetWorldEdges()
    {
        return worldEdges;
    }
}
