using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Generation using the Diamond-Square recursive algorithm, thanks for info:
// http://www.bluh.org/code-the-diamond-square-algorithm/
public class TerrainGenerator : MonoBehaviour
{
    public float roughness = 1;
    public float maxDisplacement = 2;
    public int featureStep = 16;
    public Sprite whiteSprite;
    public Color[] tintLayers;

    public bool useTestSeed = true;
    public int testSeed = 1000;

    float[,] height;
    int[,] ranges;
    int size;
    float maxHeight, minHeight;
    List<GameObject> sprites = new List<GameObject>();

    public void Generate(int gridSize)
    {
        foreach (GameObject go in sprites)
            Destroy(go);
        sprites.Clear();

        maxHeight = float.MinValue;
        minHeight = float.MaxValue;
        size = gridSize;
        height = new float[size, size];
        ranges = new int[size, size];

        if (useTestSeed)
            Random.InitState(testSeed);

        // pre-seeding some noise in
        float displacement = maxDisplacement;
        for (int x = 0; x < size; x += featureStep)
            for (int y = 0; y < size; y += featureStep)
                Set(x, y, Random.Range(-displacement, displacement));

        int iterSize = featureStep;
        while(iterSize > 1)
        {
            Iterate(iterSize, displacement);
            displacement *= Mathf.Pow(2, -roughness);
            iterSize /= 2;
        }

        // before we begin spawning tiles, look for min/max
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                maxHeight = Mathf.Max(maxHeight, Get(x, y));
                minHeight = Mathf.Min(minHeight, Get(x, y));
            }
        }

        // after iteration is done, need to place everything
        GridManager grid = GridManager.Instance;
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                PutTerrain(x, y, grid.GetCenterAtIndex(x, y));
    }

    void Iterate(int step, float displacement)
    {
        int halfStep = step / 2;
        for(int x=halfStep; x<size - 1 + halfStep; x += step)
            for(int y=halfStep; y<size - 1 + halfStep; y += step)
                Square(x, y, step, displacement);

        for (int x = 0; x < size - 1; x += step)
        {
            for (int y = 0; y < size - 1; y += step)
            {
                Diamond(x + halfStep, y, step, displacement);
                Diamond(x, y + halfStep, step, displacement);
            }
        }
    }

    void Square(int x, int y, int step, float displacement)
    {
        // a     b
        //
        //    x
        //
        // c     d

        int halfStep = step / 2;
        float a = Get(x - halfStep, y + halfStep);
        float b = Get(x + halfStep, y + halfStep);
        float c = Get(x - halfStep, y - halfStep);
        float d = Get(x + halfStep, y - halfStep);
        float val = (a + b + c + d) / 4f + Random.Range(-displacement, displacement);
        Set(x, y, val);
    }

    void Diamond(int x, int y, int step, float displacement)
    {
        //    b
        //
        // a  x  c
        //
        //    d

        int halfStep = step / 2;
        float a = Get(x - halfStep, y);
        float b = Get(x, y + halfStep);
        float c = Get(x + halfStep, y);
        float d = Get(x, y - halfStep);
        float val = (a + b + c + d) / 4f + Random.Range(-displacement, displacement);
        Set(x, y, val);
    }

    // get height for a cell at indices p
    public float Get(Vector2 p)
    {
        return height[(int)p.x, (int)p.y];
    }

    // get height range at indices p
    public int GetRange(Vector2 p)
    {
        return ranges[(int)p.x, (int)p.y];
    }

    float Get(int x, int y)
    {
        // for better results allowing wrapping
        return height[x & (size - 1), y & (size - 1)];
    }

    void Set(int x, int y, float val)
    {
        // for better results allowing wrapping
        height[x & (size - 1), y & (size - 1)] = val;
    }

    void PutTerrain(int x, int y, Vector2 pos)
    {
        // creating a general sprite only object
        GameObject go = new GameObject();
        go.transform.position = pos;
        SpriteRenderer spRenderer = go.AddComponent<SpriteRenderer>();
        spRenderer.sprite = whiteSprite;
        spRenderer.sortingOrder = -1;
        sprites.Add(go);

        // assigning it's color based on the height
        float h = height[x, y];
        int index = RangeIndex(h, minHeight, maxHeight, tintLayers.Length);
        spRenderer.color = tintLayers[index];
        ranges[x, y] = index;
    }

    int RangeIndex(float val, float min, float max, int rangeCount)
    {
        float rangeStep = (max - min) / rangeCount;
        int index = 0;
        for(float start = min; start < max; start += rangeStep)
        {
            if (val >= start && val < start + rangeStep)
                break;
            index++;
        }
        return Mathf.Min(index, rangeCount - 1); // due to floating point inaccuracies can get rangeCount as index - this safeguards against it
    }
}
