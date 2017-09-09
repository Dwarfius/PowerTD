using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Energy : MonoBehaviour
{
    [SerializeField] float maxEnergy = 100;
    [SerializeField] float energy = 100;
    public Color maxColor = Color.blue, emptyColor = Color.gray;

    public float Max 
    {
        get { return maxEnergy; }
    }
    public float Current 
    {
        get { return energy; }
        set { energy = Mathf.Min(value, maxEnergy); UpdateGraphics(); }
    }

    SpriteRenderer spRenderer;
    Color defaultColor;

    void Awake()
    {
        spRenderer = GetComponent<SpriteRenderer>();
        Building b = GetComponent<Building>();
        b.OnPlaced += () => {
            FindConnections();
            defaultColor = spRenderer.color;
            UpdateGraphics();
        };
        b.OnRemoved += () => { RemoveConnections(); };
        if (canTransfer)
            b.OnNeighborPlaced += () => { FindConnections(); };
    }

    void UpdateGraphics()
    {
        spRenderer.color = defaultColor * Color.Lerp(emptyColor, maxColor, energy / maxEnergy);
    }

    void Update()
    {
        FeedConnections();
    }

#region Transfer Logic
    public bool canTransfer = false;
    public float transferRate = 1;

    List<Energy> connections = new List<Energy>();

    void FindConnections()
    {
        if (!canTransfer)
            return;

        // supporting only 4-directional transfer
        Energy e;
        Vector2 indices = GridManager.Instance.GetIndices(transform.position);
        GameObject go = GridManager.Instance.GetAtIndices(indices + Vector2.up);
        if (go && (e = go.GetComponent<Energy>()) && !connections.Contains(e))
            connections.Add(e);

        go = GridManager.Instance.GetAtIndices(indices + Vector2.down);
        if (go && (e = go.GetComponent<Energy>()) && !connections.Contains(e))
            connections.Add(e);

        go = GridManager.Instance.GetAtIndices(indices + Vector2.right);
        if (go && (e = go.GetComponent<Energy>()) && !connections.Contains(e))
            connections.Add(e);

        go = GridManager.Instance.GetAtIndices(indices + Vector2.left);
        if (go && (e = go.GetComponent<Energy>()) && !connections.Contains(e))
            connections.Add(e);
    }

    void FeedConnections()
    {
        if (!canTransfer || Current <= 0)
            return;

        float transferAmount = transferRate * Time.deltaTime;
        foreach(Energy e in connections)
        {
            bool canFeed = e.Current < e.Max && (!e.canTransfer || Current > e.Current);
            if (canFeed)
            {
                float amount = !e.canTransfer ? transferAmount : Mathf.Min(Current - e.Current, transferAmount);
                Current -= amount;
                e.Current += amount;
            }
        }
    }

    void RemoveConnections()
    {
        connections.Clear();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        foreach (Energy e in connections)
            Gizmos.DrawLine(transform.position, e.transform.position);
    }
    #endregion
}
