using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Energy))]
public class Turret : Building
{
    public float weaponCD = 0.2f;
    public float weaponDmg = 5;
    public float weaponCost = 2;

    Energy energy;
    Health target = null;
    float lastShot = -100;

    public override void Start()
    {
        energy = GetComponent<Energy>();
        base.Start();
    }

    void Update()
    {
        if(target != null && Time.time - lastShot > weaponCD && energy.Current > 0)
        {
            energy.Current -= weaponCost;
            target.Current -= weaponDmg;
            if (target.Current <= 0)
            {
                Destroy(target.gameObject);
                target = null;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (target == null && collision.tag == "Enemy")
            target = collision.GetComponent<Health>();
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (target == null && collision.tag == "Enemy")
            target = collision.GetComponent<Health>();
    }
}
