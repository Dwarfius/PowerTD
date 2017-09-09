using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseTester : MonoBehaviour
{
    public GameObject enemyPrefab;
    public bool healthTesting = false;
    public bool energyTesting = false;
    public bool enemyTesting = true;

    void Update()
    {
        // health/energy modifier
        bool m1 = Input.GetMouseButtonDown(0), m2 = Input.GetMouseButtonDown(1);
        if(m1 || m2)
        {
            Vector2 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(point, Vector2.zero, 0);
            if(hit.collider != null)
            {
                if (healthTesting)
                {
                    Health health = hit.collider.GetComponent<Health>();
                    if (health != null)
                        health.Current += (m1 ? 10 : m2 ? -10 : 0);
                }
                if (energyTesting)
                {
                    Energy energy = hit.collider.GetComponent<Energy>();
                    if (energy != null)
                        energy.Current += (m1 ? 10 : m2 ? -10 : 0);
                }
            }
        }

        // enemy spawner
        if(Input.GetKeyDown(KeyCode.E))
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Instantiate(enemyPrefab, pos, Quaternion.identity);
        }

        if(Input.GetKeyDown(KeyCode.T))
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            Pathfinding.Path p = Pathfinding.Instance.AStar(Vector2.one * -25, Vector2.one * 25, false);
            watch.Stop();
            Debug.Log("Took: " + watch.ElapsedMilliseconds + "ms, explored: " + p.totalVisited + ", length: " + p.Count);
        }
    }
}
