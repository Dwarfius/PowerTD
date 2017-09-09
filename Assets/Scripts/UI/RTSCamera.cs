using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSCamera : MonoBehaviour
{
    public float zoomSpeed = 1;
    public float moveSpeed = 4;
    Camera cam;

	void Start ()
    {
        cam = GetComponent<Camera>();
	}
	
	void Update ()
    {
        // first, the edge movement
        Vector2 mPos = Input.mousePosition;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        int xDir = 0, yDir = 0;
        if (mPos.x >= screenSize.x)
            xDir = 1;
        else if (mPos.x <= 0)
            xDir = -1;

        if (mPos.y >= screenSize.y)
            yDir = 1;
        else if (mPos.y <= 0)
            yDir = -1;

        if (xDir != 0 || yDir != 0)
        {
            // need to check if we'll stay within the world bounds
            Vector3 move = new Vector2(xDir, yDir) * moveSpeed * Time.deltaTime;
            Rect bounds = GridManager.Instance.GetWorldEdges();
            if(bounds.Contains(transform.position + move))
                transform.Translate(move);
        }

        // then, the zoom in
        float scrollDelta = Input.mouseScrollDelta.y;
        if (scrollDelta != 0)
            cam.orthographicSize -= zoomSpeed * scrollDelta * Time.deltaTime;
	}
}
