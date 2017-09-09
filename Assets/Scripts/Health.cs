using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] float maxHealth = 100;
    [SerializeField] float health = 100;
    public GameObject healthUIPrefab;
    public Color maxColor = Color.green, minColor = Color.red;

	public float Max 
    {
        get { return maxHealth; }
    }
    public float Current 
    {
        get { return health; }
        set { health = Mathf.Min(value, maxHealth); UpdateGraphics(); }
    }

    GameObject healthUI;
    SpriteRenderer healthSpRenderer;

    void Start()
    {
        healthUI = Instantiate(healthUIPrefab, transform);
        healthSpRenderer = healthUI.GetComponent<SpriteRenderer>();

        // positioning under the object
        SpriteRenderer spRenderer = GetComponent<SpriteRenderer>();
        Vector3 pos = spRenderer.bounds.center - new Vector3(0, spRenderer.bounds.extents.y, 0);
        healthUI.transform.position = pos - new Vector3(0, 0.25f, 0);

        UpdateGraphics();
    }

    void UpdateGraphics()
    {
        bool notMax = health != maxHealth;
        healthUI.SetActive(notMax);
        if(notMax)
            healthSpRenderer.color = Color.Lerp(minColor, maxColor, health / maxHealth);
    }
}
