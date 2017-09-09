using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class Building : MonoBehaviour
{
    public event System.Action OnPlaced;
    public event System.Action OnRemoved;
    public event System.Action OnNeighborPlaced;

	public virtual void Start ()
    {
        GridManager.Instance.Add(gameObject);
        if(OnPlaced != null)
            OnPlaced();
    }

    public virtual void OnDestroy()
    {
        if (enabled)
        {
            GridManager.Instance.Remove(gameObject);
            if (OnRemoved != null)
                OnRemoved();
        }
    }

    public void NotifyNeighborPlaced()
    {
        if(OnNeighborPlaced != null)
            OnNeighborPlaced();
    }
}
