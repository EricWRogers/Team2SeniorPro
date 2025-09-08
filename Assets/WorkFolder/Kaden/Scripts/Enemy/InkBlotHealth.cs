using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkBlotHealth : MonoBehaviour
{
    public float maxHP = 30f;
    float hp;

    private SpriteRenderer spriteRenderer;

    // LootTable
    [Header("Loot")]
    public List<InkBlotDrops> lootTable = new List<InkBlotDrops>();

    void Awake() { hp = maxHP; spriteRenderer = GetComponentInChildren<SpriteRenderer>(); }
    public void TakeSpray(float amount)
    {
        hp -= amount;
        if (hp <= 0f) Die();
        StartCoroutine(FlashRed());
    }
    void Die()
    {
        // TODO: drop pigment / paint cans here
        foreach (InkBlotDrops inkBlotDrops in lootTable)
        {
            if (Random.Range(0f, 100f) <= inkBlotDrops.dropChance)
            {
                InstantiateLoot(inkBlotDrops.itemPrefab);
            }
            break;
        }
        Destroy(gameObject);
    }

    void InstantiateLoot(GameObject drops)
    {
        if (drops)
        {
            GameObject droppedInk = Instantiate(drops, transform.position, Quaternion.identity);

            MeshRenderer mesh = droppedInk.GetComponent<MeshRenderer>();
            if (mesh != null)
            {
                // This changes the material's color
                mesh.material.color = Color.red;
            }
        }
    }
    
     private IEnumerator FlashRed() //falsh red when taking damage
    {
        spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(1.0f); // 0.5 seconds

        spriteRenderer.color = Color.white;
    }
}
