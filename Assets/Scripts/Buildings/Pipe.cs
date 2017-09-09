using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Energy))]
public class Pipe : Building
{
    public Sprite[] sprites;

    Energy energy;
    SpriteRenderer spRenderer;

    void Awake()
    {
        OnPlaced += () => { UpdateSprite(); };
        OnNeighborPlaced += () => { UpdateSprite(); };
    }

    public override void Start()
    {
        energy = GetComponent<Energy>();
        spRenderer = GetComponent<SpriteRenderer>();
        base.Start();
    }

    void UpdateSprite()
    {
        Vector2 indices = GridManager.Instance.GetIndices(transform.position);
        bool top = GridManager.Instance.GetAtIndices(indices + Vector2.up) != null;
        bool bottom = GridManager.Instance.GetAtIndices(indices + Vector2.down) != null;
        bool right = GridManager.Instance.GetAtIndices(indices + Vector2.right) != null;
        bool left = GridManager.Instance.GetAtIndices(indices + Vector2.left) != null;
        if (top && bottom && right && left)
            spRenderer.sprite = sprites[0];
        else if (top && right && left)
            spRenderer.sprite = sprites[1];
        else if (top && right && bottom)
            spRenderer.sprite = sprites[2];
        else if (top && left && bottom)
            spRenderer.sprite = sprites[3];
        else if (left && right && bottom)
            spRenderer.sprite = sprites[4];
        else if (top && bottom)
            spRenderer.sprite = sprites[5];
        else if (left && right)
            spRenderer.sprite = sprites[6];
        else if (top && right)
            spRenderer.sprite = sprites[7];
        else if (right && bottom)
            spRenderer.sprite = sprites[8];
        else if (bottom && left)
            spRenderer.sprite = sprites[9];
        else if (left && top)
            spRenderer.sprite = sprites[10];
        else if (left || right)
            spRenderer.sprite = sprites[6];
        else if (top || bottom)
            spRenderer.sprite = sprites[5];
    }
}
