
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateTerrain : MonoBehaviour
{
    private Perlin perlin;

    [SerializeField]
    private GameObject blockPrefab;//use a unit cube (1x1x1 like unity's default cube)

    [SerializeField]
    private int chunkSize = 50;

    [SerializeField]
    private float noiseScale = .05f;

    [SerializeField, Range(0, 1)]
    private float threshold = .5f;

    [SerializeField]
    private Material material;

    [SerializeField]
    private bool sphere = false;

    private List<Mesh> meshes = new List<Mesh>();//used to avoid memory issues

    private void Generate()
    {
        float startTime = Time.realtimeSinceStartup;

        #region Create Mesh Data

        List<CombineInstance> blockData = new List<CombineInstance>();//this will contain the data for the final mesh
        MeshFilter blockMesh = Instantiate(blockPrefab, Vector3.zero, Quaternion.identity).GetComponent<MeshFilter>();//create a unit cube and store the mesh from it

        //go through each block position
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {

                    float noiseValue = Perlin3D(x * noiseScale, y * noiseScale, z * noiseScale);//get value of the noise at given x, y, and z.
                    if (noiseValue >= threshold)
                    {//is noise value above the threshold for placing a block?

                        //ignore this block if it's a sphere and it's outside of the radius (ex: in the corner of the chunk, outside of the sphere)
                        //distance between the current point with the center point. if it's larger than the radius, then it's not inside the sphere.
                        float raduis = chunkSize / 2;
                        if (sphere && Vector3.Distance(new Vector3(x, y, z), Vector3.one * raduis) > raduis)
                            continue;

                        blockMesh.transform.position = new Vector3(x, y, z);//move the unit cube to the intended position
                        CombineInstance ci = new CombineInstance
                        {//copy the data off of the unit cube
                            mesh = blockMesh.sharedMesh,
                            transform = blockMesh.transform.localToWorldMatrix,
                        };
                        blockData.Add(ci);//add the data to the list
                    }

                }
            }
        }

        Destroy(blockMesh.gameObject);//original unit cube is no longer needed. we copied all the data we need to the block list.

        #endregion

        #region Separate Mesh Data

        //divide meshes into groups of 65536 vertices. Meshes can only have 65536 vertices so we need to divide them up into multiple block lists.

        List<List<CombineInstance>> blockDataLists = new List<List<CombineInstance>>();//we will store the meshes in a list of lists. each sub-list will contain the data for one mesh. same data as blockData, different format.
        int vertexCount = 0;
        blockDataLists.Add(new List<CombineInstance>());//initial list of mesh data
        for (int i = 0; i < blockData.Count; i++)
        {//go through each element in the previous list and add it to the new list.
            vertexCount += blockData[i].mesh.vertexCount;//keep track of total vertices
            if (vertexCount > 65536)
            {//if the list has reached it's capacity. if total vertex count is more then 65536, reset counter and start adding them to a new list.
                vertexCount = 0;
                blockDataLists.Add(new List<CombineInstance>());
                i--;
            }
            else
            {//if the list hasn't yet reached it's capacity. safe to add another block data to this list 
                blockDataLists.Last().Add(blockData[i]);//the newest list will always be the last one added
            }
        }

        #endregion

        #region Create Mesh

        //the creation of the final mesh from the data.

        Transform container = new GameObject("Meshys").transform;//create container object
        foreach (List<CombineInstance> data in blockDataLists)
        {//for each list (of block data) in the list (of other lists)
            GameObject g = new GameObject("Meshy");//create gameobject for the mesh
            g.transform.parent = container;//set parent to the container we just made
            MeshFilter mf = g.AddComponent<MeshFilter>();//add mesh component
            MeshRenderer mr = g.AddComponent<MeshRenderer>();//add mesh renderer component
            mr.material = material;//set material to avoid evil pinkness of missing texture
            mf.mesh.CombineMeshes(data.ToArray());//set mesh to the combination of all of the blocks in the list
            meshes.Add(mf.mesh);//keep track of mesh so we can destroy it when it's no longer needed
            //g.AddComponent<MeshCollider>().sharedMesh = mf.sharedMesh;//setting colliders takes more time. disabled for testing.
        }

        #endregion

        Debug.Log("Loaded in " + (Time.realtimeSinceStartup - startTime) + " Seconds.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Destroy(GameObject.Find("Meshys"));//destroy parent gameobject as well as children.
            foreach (Mesh m in meshes)//meshes still exist even though they aren't in the scene anymore. destroy them so they don't take up memory.
                Destroy(m);
            Generate();
        }
    }

    //dunno how this works. copied it from somewhere.
    public static float Perlin3D(float x, float y, float z)
    {
        var X = Mathf.FloorToInt(x) & 0xff;
        var Y = Mathf.FloorToInt(y) & 0xff;
        var Z = Mathf.FloorToInt(z) & 0xff;
        x -= Mathf.Floor(x);
        y -= Mathf.Floor(y);
        z -= Mathf.Floor(z);
        var u = Fade(x);
        var v = Fade(y);
        var w = Fade(z);
        var A = (perm[X] + Y) & 0xff;
        var B = (perm[X + 1] + Y) & 0xff;
        var AA = (perm[A] + Z) & 0xff;
        var BA = (perm[B] + Z) & 0xff;
        var AB = (perm[A + 1] + Z) & 0xff;
        var BB = (perm[B + 1] + Z) & 0xff;
        return Lerp(w, Lerp(v, Lerp(u, Grad(perm[AA], x, y, z), Grad(perm[BA], x - 1, y, z)),
                               Lerp(u, Grad(perm[AB], x, y - 1, z), Grad(perm[BB], x - 1, y - 1, z))),
                       Lerp(v, Lerp(u, Grad(perm[AA + 1], x, y, z - 1), Grad(perm[BA + 1], x - 1, y, z - 1)),
                               Lerp(u, Grad(perm[AB + 1], x, y - 1, z - 1), Grad(perm[BB + 1], x - 1, y - 1, z - 1))));
    }
    static float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    static float Lerp(float t, float a, float b)
    {
        return a + t * (b - a);
    }

    static float Grad(int hash, float x)
    {
        return (hash & 1) == 0 ? x : -x;
    }

    static float Grad(int hash, float x, float y)
    {
        return ((hash & 1) == 0 ? x : -x) + ((hash & 2) == 0 ? y : -y);
    }

    static float Grad(int hash, float x, float y, float z)
    {
        var h = hash & 15;
        var u = h < 8 ? x : y;
        var v = h < 4 ? y : (h == 12 || h == 14 ? x : z);
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }

    static int[] perm = {
        151,160,137,91,90,15,
        131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
        190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
        88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
        77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
        102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
        135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
        5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
        223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
        129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
        251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
        49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
        138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180,
        151
    };
}