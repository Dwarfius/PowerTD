using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class Enemy : MonoBehaviour
{
    public bool debugArrows = false;
    public bool debugCosts = true;
    public GUIStyle debugLabelStyle;

    Pathfinding.Path currentPath = null;
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            Vector2 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Pathfinding.Instance.FindPath(transform.position, target, (Pathfinding.Path path) => {
                watch.Stop();
                currentPath = path;
                Debug.Log("Took " + watch.ElapsedMilliseconds + "ms for length " + currentPath.Count + " visited: " + path.totalVisited);
            }, debugArrows || debugCosts);
        }
    }

    void OnGUI()
    {
        if(debugCosts && currentPath != null && currentPath.hasDebug)
        {
            Vector2 size = Vector2.one * 20;
            GUI.skin.label = debugLabelStyle;
            GUI.skin.label.normal.textColor = Color.black;
            foreach (Pathfinding.NodeMetaInfo nodeInfo in currentPath.allVisited)
            {
                Vector2 pos = GridManager.Instance.GetCenterAtIndex((int)nodeInfo.node.loc.x, (int)nodeInfo.node.loc.y);
                Vector2 screenPos = Camera.main.WorldToScreenPoint(pos);
                screenPos.y = Screen.height - screenPos.y;
                GUI.Label(new Rect(screenPos - size / 2, size), nodeInfo.costFromStart.ToString("0.0"));
            }
            GUI.skin.label.normal.textColor = Color.white;
        }
    }

    void OnDrawGizmos()
    {
        if(currentPath != null)
        {
            // start
            Gizmos.color = Color.blue;
            Vector2 node = currentPath[0];
            Gizmos.DrawCube(node, Vector3.one);

            // drawing out all other checked nodes
            if (debugArrows && currentPath.hasDebug)
            {
                Gizmos.color = Color.white;
                foreach (Pathfinding.NodeMetaInfo nodeInfo in currentPath.allVisited)
                {
                    if (nodeInfo.parent == null)
                        continue;

                    Vector2 a = GridManager.Instance.GetCenterAtIndex((int)nodeInfo.node.loc.x, (int)nodeInfo.node.loc.y);
                    Vector2 b = GridManager.Instance.GetCenterAtIndex((int)nodeInfo.parent.loc.x, (int)nodeInfo.parent.loc.y);

                    Arrow(b, a);
                }
            }

            // overlaying actual path on top
            Gizmos.color = Color.black;
            for(int i=0; i<currentPath.Count - 1; i++)
            {
                Vector2 a = currentPath[i];
                Vector2 b = currentPath[i + 1];

                Gizmos.DrawLine(a, b);
            }

            // end
            Gizmos.color = Color.red;
            node = currentPath[currentPath.Count - 1];
            Gizmos.DrawCube(node, Vector3.one);
        }
    }

    public void Arrow(Vector3 from, Vector3 to, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Vector3 direction = to - from;
        Gizmos.DrawRay(from, direction);
        
        const float rad = 0.1f;
        Gizmos.DrawSphere(to - direction.normalized * rad, rad);
    }
}
