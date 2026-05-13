using UnityEngine;

[ExecuteInEditMode]
public class GridGenerator : MonoBehaviour
{
    public int gridSize = 5;
    public float spacing = 1f;
    public float height = 3f;

    void Update()
    {
        if (!Application.isPlaying)
        {
            GenerateGrid();
        }
    }

    void GenerateGrid()
    {
        // Delete old children
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        float offset = (gridSize - 1) / 2f;

        // Create new grid
        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                GameObject point = new GameObject($"Point_{x}_{z}");
                point.transform.parent = transform;

                point.transform.localPosition = new Vector3(
                    (x - offset) * spacing,
                    height,
                    (z - offset) * spacing
                );
            }
        }
    }
}
