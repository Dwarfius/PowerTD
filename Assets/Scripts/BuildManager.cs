using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public List<GameObject> buildings;

    GameObject locatorObj;
    SpriteRenderer spRenderer;
    bool buildMode;
    bool destroyMode;

	void Update ()
    {
        // build mode
        if (!buildMode && Input.GetKeyDown(KeyCode.B))
        {
            buildMode = true;
            locatorObj = Instantiate(buildings[0]);
            spRenderer = locatorObj.GetComponent<SpriteRenderer>();
            locatorObj.GetComponent<Building>().enabled = false;
        }
        if (buildMode && Input.GetKeyDown(KeyCode.Escape))
        {
            buildMode = false;
            Destroy(locatorObj);
            spRenderer = null;
        }

        if(buildMode)
        {
            for(int i=0; i<9; i++)
            {
                if (i < buildings.Count && Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    Destroy(locatorObj);
                    spRenderer = null;

                    locatorObj = Instantiate(buildings[i]);
                    spRenderer = locatorObj.GetComponent<SpriteRenderer>();
                    locatorObj.GetComponent<Building>().enabled = false;
                }
            }
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            locatorObj.transform.position = GridManager.Instance.GetCenter(mousePos);
            bool canBuild = GridManager.Instance.IsFree(mousePos);
            spRenderer.color = canBuild ? Color.green : Color.red;
            if(canBuild && Input.GetMouseButtonDown(0))
            {
                spRenderer.color = Color.white;
                locatorObj.GetComponent<Building>().enabled = true;
                buildMode = false;
                spRenderer = null;
                locatorObj = null;
            }
        }

        // destroy mode
        if(!buildMode && !destroyMode && Input.GetKeyDown(KeyCode.X))
            destroyMode = true;
        if(destroyMode && Input.GetKeyDown(KeyCode.Escape))
            destroyMode = false;
        if(destroyMode && Input.GetMouseButtonDown(0))
        {
            Vector2 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(point, Vector2.zero, 0);
            if (hit.collider != null && hit.collider.GetComponent<Building>())
                Destroy(hit.collider.gameObject);
        }
	}
}
