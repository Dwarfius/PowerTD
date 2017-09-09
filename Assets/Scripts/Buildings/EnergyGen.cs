using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Energy))]
public class EnergyGen : Building
{
    public float rate = 1;

    Energy energy;

    public override void Start()
    {
        energy = GetComponent<Energy>();
        base.Start();
    }

    void Update()
    {
        energy.Current += rate * Time.deltaTime;
	}
}
