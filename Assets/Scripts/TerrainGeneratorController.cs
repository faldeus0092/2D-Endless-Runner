using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneratorController : MonoBehaviour
{
    [Header("Templates")]
    public float terrainTemplateWidth;

    [Header("Generator Area")]
    public Camera gameCamera;
    public float areaStartOffset;
    public float areaEndOffset;

    [Header("Force Early Template")]
    public List<TerrainTemplateController> earlyTerrainTemplates;
    public List<TerrainTemplateController> terrainTemplates;
    private List<GameObject> spawnedTerrain;
    private const float debugLineHeight = 10.0f;

    // pool
    private Dictionary<string, List<GameObject>> pool;
    
    // area start
    private float GetHorizontalPositionStart()
    {
        return gameCamera.ViewportToWorldPoint(new Vector2(0f, 0f)).x + areaStartOffset;
    }

    private float GetHorizontalPositionEnd()
    {
        return gameCamera.ViewportToWorldPoint(new Vector2(1f, 0f)).x + areaEndOffset;
    }


    // debug
    private void OnDrawGizmos()
    {
        Vector3 areaStartPosition = transform.position;
        Vector3 areaEndPosition = transform.position;

        areaStartPosition.x = GetHorizontalPositionStart();
        areaEndPosition.x = GetHorizontalPositionEnd();

        Debug.DrawLine(areaStartPosition + Vector3.up * debugLineHeight / 2, areaStartPosition + Vector3.down * debugLineHeight / 2, Color.red);
        Debug.DrawLine(areaEndPosition + Vector3.up * debugLineHeight / 2, areaEndPosition + Vector3.down * debugLineHeight / 2, Color.red);
    }


    private float lastGeneratedPositionX;
    private float lastRemovedPositionX;

    // Start is called before the first frame update
    void Start()
    {
        pool = new Dictionary<string, List<GameObject>>();
        spawnedTerrain = new List<GameObject>();

        lastGeneratedPositionX = GetHorizontalPositionStart();
        lastRemovedPositionX = lastGeneratedPositionX - terrainTemplateWidth;


        foreach (TerrainTemplateController terrain in earlyTerrainTemplates)
        {
            GenerateTerrain(lastGeneratedPositionX, terrain);
            lastGeneratedPositionX += terrainTemplateWidth;
        }

        // generate terrain selama masih ada sisa di layar
        while (lastGeneratedPositionX < GetHorizontalPositionEnd())
        {
            GenerateTerrain(lastGeneratedPositionX);
            lastGeneratedPositionX += terrainTemplateWidth;
        }
    }

    // fungsi pool
    private GameObject GenerateFromPool(GameObject item, Transform parent)
    {
        if (pool.ContainsKey(item.name))
        {
            // Jika item ada di pool
            if (pool[item.name].Count > 0)
            {
                GameObject newItemFromPool = pool[item.name][0];
                pool[item.name].Remove(newItemFromPool);
                newItemFromPool.SetActive(true);
                return newItemFromPool;
            }
        }
        else
        {
            // Buat baru
            pool.Add(item.name, new List<GameObject>());
        }
        
        GameObject newItem = Instantiate(item, parent);
        newItem.name = item.name;
        return newItem;
    }

    private void ReturnToPool(GameObject item)
    {
        if (!pool.ContainsKey(item.name))
        {
            Debug.LogError("INVALID POOL ITEM!!");
        }

        pool[item.name].Add(item);
        item.SetActive(false);
    }

    // generate terrain pada posisi X
    private void GenerateTerrain(float posX, TerrainTemplateController forceterrain = null)
    {
        GameObject newTerrain = GenerateFromPool(terrainTemplates[Random.Range(0, terrainTemplates.Count)].gameObject, transform);

        newTerrain.transform.position = new Vector2(posX, 0f);

        spawnedTerrain.Add(newTerrain);
    }

    // Update is called once per frame
    void Update()
    {
        // generate yang sama pada fungsi start
        while (lastGeneratedPositionX < GetHorizontalPositionEnd())
        {
            GenerateTerrain(lastGeneratedPositionX);
            lastGeneratedPositionX += terrainTemplateWidth;
        }
        // menghapus terrain yang lewat layar
        while (lastRemovedPositionX + terrainTemplateWidth < GetHorizontalPositionStart())
        {
            lastRemovedPositionX += terrainTemplateWidth;
            RemoveTerrain(lastRemovedPositionX);
        }
    }

    private void RemoveTerrain(float posX)
    {
        GameObject terrainToRemove = null;

        // terrain di posisi x
        foreach (GameObject item in spawnedTerrain)
        {
            if (item.transform.position.x == posX)
            {
                terrainToRemove = item;
                break;
            }
        }

        // after found;
        if (terrainToRemove != null)
        {
            spawnedTerrain.Remove(terrainToRemove);
            ReturnToPool(terrainToRemove);
        }
    }
}
