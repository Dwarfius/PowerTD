using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

// extending specialized List<T> for convenience
public static class Utils
{
    // assuming the array is sorted, insert a node retaining sorted order using binary search
    public static void AddSorted(this List<Pathfinding.NodeMetaInfo> nodes, Pathfinding.NodeMetaInfo newNode)
    {
        int left = 0, right = nodes.Count;
        while(right > left)
        {
            int center = (left + right) / 2;
            if (nodes[center].totalCost <= newNode.totalCost)
                left = center + 1;
            else
                right = center;
        }
        nodes.Insert(left, newNode);
    }

    // binary search for index of an element, uses RefEquals
    public static int FindIndexSorted(this List<Pathfinding.NodeMetaInfo> nodes, Pathfinding.NodeMetaInfo node)
    {
        int left = 0, right = nodes.Count;
        int index = -1;
        while (right > left)
        {
            int center = (left + right) / 2;

            if (nodes[center].totalCost == node.totalCost)
            {
                index = center;
                break;
            }
            else if (nodes[center].totalCost < node.totalCost)
                left = center + 1;
            else
                right = center;
        }

        if (index > -1)
        {
            // we found one of the occurances, now need to go through the range of em and check every one
            // to the right
            int i = index;
            while (i < nodes.Count && nodes[i].totalCost == node.totalCost)
                if (ReferenceEquals(nodes[i++], node))
                    return i - 1;
            // to the left
            i = index - 1;
            while (i >= 0 && nodes[i].totalCost == node.totalCost)
                if (ReferenceEquals(nodes[i--], node))
                    return i + 1;
        }
        return index;
    }

    // moves the meta info to a new position, uses RefEquals
    public static void UpdateInSorted(this List<Pathfinding.NodeMetaInfo> nodes, Pathfinding.NodeMetaInfo newNode, float oldTotalCost)
    {
        float newTotalCost = newNode.totalCost;
        newNode.totalCost = oldTotalCost;
        int ind = nodes.FindIndexSorted(newNode);
        newNode.totalCost = newTotalCost;

        if (ind > 0 && newNode.totalCost < nodes[ind - 1].totalCost) // need to swap left (shift right)
        {
            while (ind > 0 && newNode.totalCost < nodes[ind - 1].totalCost)
            {
                nodes[ind] = nodes[ind - 1];
                ind--;
            }
            nodes[ind] = newNode;
        }
        else if (ind < nodes.Count - 1 && newNode.totalCost > nodes[ind + 1].totalCost) // need to swap right (shift left)
        {
            while (ind < nodes.Count - 1 && newNode.totalCost > nodes[ind + 1].totalCost)
            {
                nodes[ind] = nodes[ind + 1];
                ind++;
            }
            nodes[ind] = newNode;
        }
    }

    // using binary search check for node, uses RefEquals
    public static bool ContainsSorted(this List<Pathfinding.NodeMetaInfo> nodes, Pathfinding.NodeMetaInfo node)
    {
        return nodes.FindIndexSorted(node) != -1;
    }

    // Debug utility
    public static bool CheckSorted(this List<Pathfinding.NodeMetaInfo> nodes)
    {
        for(int i=0; i<nodes.Count - 1; i++)
        {
            Pathfinding.NodeMetaInfo n1, n2;
            n1 = nodes[i];
            n2 = nodes[i + 1];
            if (n1.totalCost > n2.totalCost)
                return false;
        }
        return true;
    }
}

public class Pathfinding
{
    public class Path : List<Vector2>
    {
        public List<NodeMetaInfo> allVisited;
        public int totalVisited;
        public bool hasDebug;
    }

    public class Node
    {
        public Vector2 loc;
        public int heightRange; // stores range slices instead of actual height, for simple heuristic processing
        public List<Node> neighbours;

        public override string ToString() { return loc.ToString(); }

        // since we create them during iterations (and not initially), we need to overwrite those to
        // make sure ContainsKey will be able to pick up similar, yet distinct objects as the same
        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(Node))
                return Equals((Node)obj);
            return base.Equals(obj);
        }
        public bool Equals(Node other) { return loc.Equals(other.loc); }
        public override int GetHashCode() { return loc.GetHashCode(); }
        public static bool operator==(Node n1, Node n2)
        {
            if (ReferenceEquals(n1, n2))
                return true;
            if (((object)n1 == null) || ((object)n2 == null))
                return false;
            return n1.Equals(n2);
        }
        public static bool operator!=(Node n1, Node n2) { return !(n1 == n2); }
    }

    // this is an internal utility class, holds single node calculated values - helps with multithreading
    public class NodeMetaInfo
    {
        public float costFromStart, estCostToEnd, totalCost;
        public Node node, parent;
        public int QueueIndex; // QueueIndex is related to the fastqueue implementation

        public override string ToString() { return totalCost.ToString() + " " + node.ToString(); }
    }

    public static Pathfinding Instance 
    {
        get 
        {
            if (instance == null)
                instance = new Pathfinding();
            return instance;
        }
    }
    static Pathfinding instance = null;

    GridManager gridManager;
    TerrainGenerator terrainGen;

    public void Init(GridManager man)
    {
        gridManager = man;
        terrainGen = man.GetComponent<TerrainGenerator>();

        Pregenerate();
    }

    // Given world coordinates, find a path from Pos to Target, and execute an action with resulting Path.
    public void FindPath(Vector2 pos, Vector2 target, System.Action<Path> callback, bool debug = false)
    {
        // schedule a task for calculating a new path
        Task<Path> pathTask = Task.Factory.StartNew(() => { return AStar(pos, target, debug); }, TaskCreationOptions.PreferFairness);
        pathTask.ContinueWith((Task<Path> finishedTask) => {
            // the task has completed - check if we have a valid path
            Path result = finishedTask.Result;
            if (result != null) 
            {
                // need to transform it in to world space
                for (int i = 0; i < result.Count; i++)
                    result[i] = GridManager.Instance.GetCenterAtIndex((int)result[i].x, (int)result[i].y);
                // we do - execute the provided callback
                Dispatcher.Instance.Add(() => { callback(result); });
                // TODO: in the future might be worth to pool/cache the path
                // if we have a lot of similar paths queued
            }
        });
    }

    public Path AStar(Vector2 pos, Vector2 target, bool debug)
    {
        try
        {
            int maxSize = gridManager.gridSize * gridManager.gridSize;
            //int maxSize = (int)((target-pos).sqrMagnitude / 2);
            List<NodeMetaInfo> toExplore = new List<NodeMetaInfo>(maxSize); // might be a good candidate for pooling if we have constant requests
            Dictionary<Vector2, NodeMetaInfo> nodeMetas = new Dictionary<Vector2, NodeMetaInfo>(maxSize); // need to track costFromStart separatelly, since we're recreating nodes on the fly

            // we're looking for a path in grid space, so have to convert it
            Vector2 targetInd = gridManager.GetIndices(target);
            Node targetNode = new Node();
            targetNode.loc = targetInd;
            {
                Node startNode = new Node();
                startNode.loc = gridManager.GetIndices(pos);

                NodeMetaInfo metaInfo = new NodeMetaInfo();
                metaInfo.costFromStart = 0;
                metaInfo.estCostToEnd = EstimatedCostToTarget(startNode, targetNode);
                metaInfo.totalCost = metaInfo.costFromStart + metaInfo.estCostToEnd;
                metaInfo.parent = null;
                metaInfo.node = startNode;

                toExplore.Add(metaInfo);
                nodeMetas[startNode.loc] = metaInfo;
            }

            Path path = null;
            Rect bounds = new Rect(Vector2.zero, Vector2.one * gridManager.gridSize);
            while (toExplore.Count > 0)
            {
                NodeMetaInfo nodeMeta = toExplore[0];
                toExplore.RemoveAt(0);
                Node node = nodeMeta.node;
                if (node.loc == targetInd)
                {
                    // we found our path - need to unroll it
                    path = new Path();
                    path.hasDebug = debug;
                    while (node != null)
                    {
                        path.Add(node.loc);
                        node = nodeMetas[node.loc].parent;
                    }
                    path.Reverse();
                    path.totalVisited = nodeMetas.Count;

                    if (debug)
                    {
                        path.allVisited = new List<NodeMetaInfo>();
                        foreach (NodeMetaInfo info in nodeMetas.Values)
                            path.allVisited.Add(info);
                    }
                    break;
                }
                else
                {
                    // probably not the best way to do it, but don't see a better way right now
                    for (int x = -1; x < 2; x++)
                    {
                        for (int y = -1; y < 2; y++)
                        {
                            if (x == 0 && y == 0) // avoid creating the same node
                                continue;

                            Vector2 newPos = node.loc + new Vector2(x, y);
                            if (!bounds.Contains(newPos)) // avoid out of bounds cases
                                continue;

                            Node newNode = new Node();
                            newNode.loc = newPos;
                            NodeMetaInfo newNodeMeta;
                            
                            // checking to see whether the new cost that we calculated for this is better
                            float newCost = nodeMeta.costFromStart + CostOfSingleStep(node, newNode);
                            bool inSorted = false;
                            float oldTotalCost = 0;
                            if(nodeMetas.TryGetValue(newNode.loc, out newNodeMeta))
                            {
                                // we visited this node before - means we have calculated the cost before once
                                // check whether it's faster to get to new node from current position, rather than old
                                if (newCost < newNodeMeta.costFromStart)
                                {
                                    // do early search (relies on totalCost internally for lookup)
                                    inSorted = toExplore.ContainsSorted(newNodeMeta);
                                    oldTotalCost = newNodeMeta.totalCost;

                                    // the node already has an estimate to target calculated, and current node set
                                    // meaning we need to update only part of the info
                                    newNodeMeta.parent = node;
                                    newNodeMeta.costFromStart = newCost;
                                    newNodeMeta.totalCost = newNodeMeta.costFromStart + newNodeMeta.estCostToEnd;

                                    if (inSorted)
                                        toExplore.UpdateInSorted(newNodeMeta, oldTotalCost);
                                    else
                                        toExplore.AddSorted(newNodeMeta);
                                }
                            }
                            else
                            {
                                newNodeMeta = new NodeMetaInfo();
                                newNodeMeta.costFromStart = newCost;
                                nodeMetas[newNode.loc] = newNodeMeta;

                                // this node is explored for the first time
                                // either way, update it's information
                                newNodeMeta.node = newNode;
                                newNodeMeta.parent = node;
                                newNodeMeta.estCostToEnd = EstimatedCostToTarget(newNode, targetNode);
                                newNodeMeta.totalCost = newNodeMeta.costFromStart + newNodeMeta.estCostToEnd;

                                toExplore.AddSorted(newNodeMeta);
                            }
                        }
                    }
                }
            }

            return path;
        }
        catch(System.Exception e)
        {
            Dispatcher.Instance.Add(() => { Debug.LogError(e.ToString()); });
            return null;
        }
    }

    float EstimatedCostToTarget(Node from, Node to)
    {
        const float directMod = 1;
        const float diagonMod = 1.4f;

        // using diagonal distance - thanks http://theory.stanford.edu/~amitp/GameProgramming/Heuristics.html
        float dx = Mathf.Abs(to.loc.x - from.loc.x);
        float dy = Mathf.Abs(to.loc.y - from.loc.y);
        return directMod * (dx + dy) + (diagonMod - 2 * directMod) * Mathf.Min(dx, dy);
    }

    float CostOfSingleStep(Node from, Node to)
    {
        const float directMod = 1;
        const float diagonMod = 1.4f;

        float mod = (from.loc.x != to.loc.x && from.loc.y != to.loc.y) ? diagonMod : directMod;
        int fromRange = terrainGen.GetRange(from.loc);
        int toRange = terrainGen.GetRange(to.loc);
        return Mathf.Abs(toRange - fromRange) * 4 + mod;
    }

    void Pregenerate()
    {
        // TODO: pregenerate the graph

    }
}
