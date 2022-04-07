/******************************************************************************
 *
 *                      GeaVR
 *                https://www.geavr.eu/
 *             https://github.com/GeaVR/GeaVR
 * 
 * GeaVR is an open source software that allows the user to experience a wide 
 * range of geological and geomorphological sites in immersive virtual reality,
 * including data collection.
 *
 * Main Developers:      
 * 
 *     Fabio Luca Bonali (fabio.bonali@unimib.it)
 *     Martin Kearl (martintkearl@gmail.com)
 *     Fabio Roberto Vitello (fabio.vitello@inaf.it)
 * 
 * Developed thanks to the contribution of following projects:
 *
 *     ACPR15T4_ 00098 “Agreement between the University of Milan Bicocca and the 
 *     Cometa Consortium for the experimentation of cutting-edge interactive 
 *     technologies for the improvement of science teaching and dissemination” of 
 *     Italian Ministry of Education, University and Research (ARGO3D)
 *     PI: Alessandro Tibaldi (alessandro.tibaldi@unimib.it)
 *     
 *     Erasmus+ Key Action 2 2017-1-UK01-KA203- 036719 “3DTeLC – Bringing the  
 *     3D-world into the classroom: a new approach to Teaching, Learning and 
 *     Communicating the science of geohazards in terrestrial and marine 
 *     environments”
 *     PI: Malcolm Whitworth (malcolm.Whitworth@port.ac.uk)
 * 
 ******************************************************************************
 * Copyright (c) 2016-2022
 * GPL-3.0 License
 *****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using UnityEngine;

/*
* A component for the loading of a single object
*/
public class ThreadedOBJLoader : MonoBehaviour
{
    /* boolean to determine if the SOBJ has been loading*/
    public bool loadingCompleted = false;

    /* Handle for the loading thread */
    private Thread LoadingThread;

    /* List of loaded objects, 1 element per file loaded */
    private List<StreamedObjData> ObjStreams = new List<StreamedObjData>();

    /* path to file to load */
    public string filePath = "";

    /* component will attempt load after initialisation */
    public bool autoRun = false;

    /* component will create a collider for loaded objects */
    public bool generateColliders = false;

    /* faster load times 1.5X -> 2X , but causes framerate hitches*/
    public bool blockOnLoad = false;

    /* is component ready for deletion */
    [HideInInspector]
    public bool ShouldDelete = false;

    /* which LOD level this SOBJ belongs to */
    [HideInInspector]
    public int lodLevel = 0;
    
    /* unique index for each SOBJ */
    [HideInInspector]
    public int modelIndex = 0;

    /* List of all game objects created for this SOBJ */
    [HideInInspector]
    public List<GameObject> meshObjects = new List<GameObject>();

    /* List of all collider GameObjects created for this SOBJ */
    [HideInInspector]
    public List<GameObject> colliderObjects = new List<GameObject>();

    /* callback event when loading is completed */
    public event Action<ThreadedOBJLoader> OBJLoadedEvent;

    /* static variable to allow obj loaders to coordinate syncing, true if ANY ThreadedOBJLoader component is syncing */
    static private bool CheckObjLoaderSync = false;

    /* set true if this component is syncing */
    private bool isActiveLoader = false;

    /* using legacy loading */
    private bool usingLegacyLoading = true;

    /**
    * Start is called once during initialisation
    */
    void Start()
    {
        if (autoRun)
        {
            Run();
        }
    }

    /**
    * Update is called once per frame
    */
    void Update()
    {
        if (ShouldDelete)
        {
            return;
        }

        // check self to see if thread is complete
        if (loadingCompleted == true && (!CheckObjLoaderSync || isActiveLoader))
        {
            // is a sync point, but Unity Update calls are synchronous 
            // so no chance of data races.
            CheckObjLoaderSync = true;
            isActiveLoader = true;
            loadingCompleted = false;

            // start coroutine to convert loaded data to gameobjects
            if (usingLegacyLoading)
            {
                StartCoroutine(this.CreateGameObjectsFromDataLegacy_Coroutine());
            }
            else
            {
                StartCoroutine(this.CreateGameObjectsFromData_Coroutine());
            }
        }
    }

    /**
    * check if loading of the file is complete
    *
    * @return true if loading completed, else false
    */
    public bool IsValid()
    {
        return loadingCompleted;
    }

    /**
    * Remove Loader component and cleanup dependent objects
    *
    * @return true if cleanup successful, otherwise false
    */
    public bool DestroyLoader()
    {
        ShouldDelete = true;

        if (LoadingThread != null)
        {
            LoadingThread.Abort();
        }

        StopAllCoroutines();

        if (isActiveLoader)
        {
            CheckObjLoaderSync = false;
        }

        foreach (GameObject meshObj in meshObjects)
        {
            Destroy(meshObj);
        }

        foreach (GameObject colliderObj in colliderObjects)
        {
            Destroy(colliderObj);
        }

        Destroy(this);
        return ShouldDelete;
    }

    /**
    * Begin loading of file by creating a new thread 
    * runs LoadDataFromSOBJFile and LoadTextureFromFile in this thread
    */
    public void Run()
    {
        if (!ShouldDelete)
        {
            LoadingThread = new Thread(LoadDataFromSOBJFile);
            LoadingThread.Start();
        }
        else
        {
            Destroy(this);
        }
    }

    /**
    * Function to run on threads during loading
    */
    public void LoadDataFromSOBJFile()
    {
        Debug.Log("start");

        // does file exist?
        if (!File.Exists(filePath))
        {
            return;
        }

        string folderPath = Path.GetDirectoryName(filePath);

        string metaPath = filePath + ".meta";

        FileStream fs = File.Open(filePath, FileMode.Open);
        FileStream mfs = File.Open(metaPath, FileMode.Open);

        StreamReader sreader = new StreamReader(mfs);

        // extract meta data from file
        string name = "";
        string texture = "";
        int NumPhysMeshes = 0;

        int positionBytes = 0;
        int texcoordBytes = 0;
        int trianglesBytes = 0;

        List<int> physByteOffsets = new List<int>();

        try
        {
            name = sreader.ReadLine();
            texture = sreader.ReadLine();
            NumPhysMeshes = int.Parse(sreader.ReadLine());

            positionBytes = int.Parse(sreader.ReadLine());
            texcoordBytes = int.Parse(sreader.ReadLine());
            trianglesBytes = int.Parse(sreader.ReadLine());

            for (int i = 0; i < NumPhysMeshes * 2; ++i)
            {
                physByteOffsets.Add(int.Parse(sreader.ReadLine()));
            }
        }
        catch
        {
            Debug.Log("error loading meta data file");
            return;
        }
        
        // create streamed obj
        StreamedObjData currentObj = new StreamedObjData();

        currentObj.generateCollider = generateColliders;
        currentObj.name = name;

        // load binary data
        BinaryReader reader = new BinaryReader(fs);

        for (int i = 0; i < (positionBytes / 4) / 3; ++i)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();

            currentObj.positions.Add(new Vector3(x, y, z));
        }

        for (int i = 0; i < (texcoordBytes / 4) / 2; ++i)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();

            currentObj.texcoords.Add(new Vector2(x, y));
        }

        for (int i = 0; i < trianglesBytes / 4; ++i)
        {
            currentObj.triangles.Add(reader.ReadInt32());
        }

        // initialise correct number of lists
        for (int j = 0; j < NumPhysMeshes - 1; ++j)
        {
            currentObj.physPositions.Add(new List<Vector3>());
            currentObj.physTriangles.Add(new List<int>());
        }

        // fill lists
        for (int j = 0; j < NumPhysMeshes; ++j)
        {
            int physPositionBytes = physByteOffsets[j * 2];
            int physTrianglesBytes = physByteOffsets[(j * 2) + 1];

            for (int i = 0; i < (physPositionBytes / 4) / 3; ++i)
            {
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                float z = reader.ReadSingle();

                currentObj.physPositions[j].Add(new Vector3(x, y, z));
            }

            for (int i = 0; i < physTrianglesBytes / 4; ++i)
            {
                currentObj.physTriangles[j].Add(reader.ReadInt32());
            }
        }

        ObjStreams.Add(currentObj);

        // load texture to last created ObjStream
        LoadTextureFromFile(texture, folderPath);

        loadingCompleted = true;
        Debug.Log("done");
    }

    /**
    * Function for loading of DDS texture data
    */
    public void LoadTextureFromFile(string DiffuseTextureMapName, string folderPath)
    {
        //load Texture data
        string ddsTexturePath = folderPath + "\\" + Path.GetFileNameWithoutExtension(DiffuseTextureMapName) + ".dds";

        if (!File.Exists(ddsTexturePath))
        {
            Debug.Log("texture not found");
            return;
        }
        Debug.Log("texture load start");

        //ddsData
        byte[] ddsBytes = null;

        // file open check
        bool isClosed = false;
        while (!isClosed)
        {
            try
            {
                ddsBytes = File.ReadAllBytes(ddsTexturePath);
                isClosed = true;
            }
            catch (IOException)
            {
                continue;
            }
            Thread.Yield();
        }

        ObjStreams[ObjStreams.Count - 1].texturePath = ddsTexturePath;

        int DDS_HEADER_SIZE = 128;
        ObjStreams[ObjStreams.Count - 1].textureHeight = ddsBytes[13] * 256 + ddsBytes[12];
        ObjStreams[ObjStreams.Count - 1].textureWidth = ddsBytes[17] * 256 + ddsBytes[16];
        int mipMapCount = ddsBytes[29] * 256 + ddsBytes[28];

        ObjStreams[ObjStreams.Count - 1].textureCompressedByteArray = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
        Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, ObjStreams[ObjStreams.Count - 1].textureCompressedByteArray, 0, ddsBytes.Length - DDS_HEADER_SIZE);

        Thread.Yield();
    }

    /**
    * From loaded data create appropriate gameobjects
    */
    public IEnumerator CreateGameObjectsFromDataLegacy_Coroutine()
    {
        WaitForEndOfFrame wfeof = new WaitForEndOfFrame();

        foreach (StreamedObjData streamedObj in ObjStreams)
        {
            // create mesh
            Mesh polyMesh = new Mesh();
            polyMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            polyMesh.vertices = streamedObj.positions.ToArray();
            polyMesh.uv = streamedObj.texcoords.ToArray();
            polyMesh.triangles = streamedObj.triangles.ToArray();

            // create meshObject
            GameObject meshObject = new GameObject(streamedObj.name);
            meshObject.AddComponent<MeshFilter>();
            meshObject.AddComponent<MeshRenderer>();
            meshObject.GetComponent<MeshFilter>().sharedMesh = polyMesh;
            meshObjects.Add(meshObject);
            
            meshObject.transform.position = -WorldRebaser.accumulatedOffset;

            // create Texture
            Texture2D tex = new Texture2D(streamedObj.textureWidth, streamedObj.textureHeight, TextureFormat.DXT1, true);
            tex.LoadRawTextureData(streamedObj.textureCompressedByteArray);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply();

            // create Material
            Material mat = new Material(Shader.Find("Unlit/Texture"));
            mat.mainTexture = tex;

            // set Material to mesh
            meshObject.GetComponent<MeshRenderer>().material = mat;

            if (blockOnLoad == false) yield return wfeof;

            // create colliders   
            if (streamedObj.generateCollider)
            {
                for (int i = 0; i < streamedObj.physPositions.Count; ++i)
                {
                    // generate mesh
                    Mesh physMesh = new Mesh();

                    // use 32bit indices
                    physMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                    physMesh.vertices = streamedObj.physPositions[i].ToArray();
                    physMesh.triangles = streamedObj.physTriangles[i].ToArray();

                    // make child object
                    // only 1 mesh collider per GameObject, so create proxy object
                    GameObject colliderObject = new GameObject(streamedObj.name + "Collider");

                    colliderObject.transform.position = -WorldRebaser.accumulatedOffset;

                    // add collider to child
                    MeshCollider collider = colliderObject.AddComponent<MeshCollider>();
                    collider.cookingOptions = MeshColliderCookingOptions.None;
                    collider.sharedMesh = physMesh;

                    // add gameobject to list
                    colliderObjects.Add(colliderObject);
                    if (blockOnLoad == false) yield return wfeof;
                }
            }
        }

        //callback
        OBJLoadedEvent(this);
        yield return wfeof;

        //cleanup
        CheckObjLoaderSync = false;
        Destroy(this);

        //end
        yield return wfeof;
    }

    /**
   * From loaded data create appropriate gameobjects
   */
    public IEnumerator CreateGameObjectsFromData_Coroutine()
    {
        WaitForEndOfFrame wfeof = new WaitForEndOfFrame();

        foreach (StreamedObjData streamedObj in ObjStreams)
        {
            // create mesh
            Mesh polyMesh = new Mesh();
            polyMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            polyMesh.vertices = streamedObj.positions.ToArray();
            polyMesh.uv = streamedObj.texcoords.ToArray();
            polyMesh.triangles = streamedObj.triangles.ToArray();

            // create meshObject
            GameObject meshObject = new GameObject(streamedObj.name);
            meshObject.AddComponent<MeshFilter>();
            meshObject.AddComponent<MeshRenderer>();
            meshObject.GetComponent<MeshFilter>().sharedMesh = polyMesh;
            meshObjects.Add(meshObject);

            // 
            meshObject.transform.position = -WorldRebaser.accumulatedOffset;

            // create Texture
            Texture2D tex = new Texture2D(streamedObj.textureWidth, streamedObj.textureHeight, TextureFormat.DXT1, true);
            tex.LoadRawTextureData(streamedObj.textureCompressedByteArray);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply();

            // create Material
            Material mat = new Material(Shader.Find("Unlit/Texture"));
            mat.mainTexture = tex;

            // set Material to mesh
            meshObject.GetComponent<MeshRenderer>().material = mat;

            if (blockOnLoad == false) yield return wfeof;

            // create colliders   
            if (streamedObj.generateCollider)
            {
                for (int i = 0; i < streamedObj.physPositions.Count; ++i)
                {
                    // generate mesh
                    Mesh physMesh = new Mesh();

                    // use 32bit indices
                    physMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                    physMesh.vertices = streamedObj.physPositions[i].ToArray();
                    physMesh.triangles = streamedObj.physTriangles[i].ToArray();

                    // make child object
                    // only 1 mesh collider per GameObject, so create proxy object
                    GameObject colliderObject = new GameObject(streamedObj.name + "Collider");
                    colliderObject.transform.position = -WorldRebaser.accumulatedOffset;

                    // add collider to child
                    MeshCollider collider = colliderObject.AddComponent<MeshCollider>();
                    collider.cookingOptions = MeshColliderCookingOptions.None;
                    collider.sharedMesh = physMesh;

                    // add gameobject to list
                    colliderObjects.Add(colliderObject);
                    if (blockOnLoad == false) yield return wfeof;
                }
            }
        }

        //callback
        OBJLoadedEvent(this);
        yield return wfeof;

        //cleanup
        CheckObjLoaderSync = false;
        Destroy(this);

        //end
        yield return wfeof;
    }
}
