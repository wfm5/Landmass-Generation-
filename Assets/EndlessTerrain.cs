using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EndlessTerrain : MonoBehaviour {

    public const float maxViewDst = 450;
    public Transform viewer;
    
    public static Vector2 viewPosition;
    static MapGenerator mapGenerator;
    int chunkSize;
    int chunksVisibleInViewDst;
    
    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);
    }

    void Update()
    {
        viewPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks()
    {
        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewPosition.x/ chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewPosition.y/ chunkSize);

        //fors
        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++){
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
            {
                //check if vis
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    if (terrainChunkDictionary[viewedChunkCoord].IsVisible())
                    {
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
                    }
                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform));
                }
            }
        }
    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;

        public TerrainChunk(Vector2 coord, int size, Transform parent)
        {
            position = coord * size;
            bounds = new Bounds(position,Vector2.one*size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();

            meshObject.transform.position = positionV3;
            meshObject.transform.localScale = Vector3.one * size / 10f;
            meshObject.transform.parent = parent;
            SetVisible(false);

            mapGenerator.RequestMapData(OnMapDataReceived);
        }
        void OnMapDataReceived(MapData mapData)
        {
            mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);

        }
        void OnMeshDataReceived(MeshData meshData)
        {
            meshFilter.mesh = meshData.CreateMesh();
        }
        public void UpdateTerrainChunk()
        {
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewPosition));
            bool visible = viewerDstFromNearestEdge <=maxViewDst;
            SetVisible(visible);
        }
        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }
        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }
    
}
