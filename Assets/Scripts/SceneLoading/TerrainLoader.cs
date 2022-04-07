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

using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using SharpKml.Dom;
using SharpKml.Engine;
using System.Linq;
using System.Xml;
using System.Globalization;
using System.Collections;

/*
* A component for the loading of terrain from folders of SOBJ files
*/
public class TerrainLoader : MonoBehaviour
{
    /* non static Folder to import from */
    public string importFolder = "";

    /* Folder to import from */
    public static string importLocation = "";

    /* Number of cores to use, -1 uses all available */
    [Tooltip("Number of cores to use, -1 uses all available.")]
    public int numberOfThreads = -1;

    /* Load from importLocation on game start */
    public bool autoRun = false;

    /* Create colliders for loaded objects */
    public bool generateColliders = false;

    /* only create colliders for lowest LOD level */
    [Tooltip("Only create colliders for lowest LOD level.")]
    public bool onlyGenerateSimpleColliders = false;

    /* faster load times 1.5X -> 2X , but causes framerate hitches*/
    [Tooltip("faster load times 1.5X -> 2X, but causes framerate hitches.")]
    public bool blockOnLoad = false;

    /* reference to position controller for setting GPS data */
    public PositionController positionController;

    /* reference to player for setting position at start */
    [Tooltip("Reference to GameObject to move once the scene is loaded.")]
    public GameObject player = null;

    /* reference to player for setting position at start */
    [Tooltip("Reference to UI object to show whilst loading.")]
    public GameObject LoadingIcon = null;

    /* faster load times 1.5X -> 2X , but causes framerate hitches*/
    [Tooltip("faster load times 1.5X -> 2X, but causes framerate hitches.")]
    public bool hideBoxOnAwaken = true;

    /* has player been placed */
    private bool playerPlaced = false;

    /* player start location */
    public static Vector3 playerStart = new Vector3(-999999, -999999, -999999);

    /* maximum number of tiles loaded around player */
    public int loadedGridDistance = 5;

    [HideInInspector]
    private LODGroup lodGroup;

    [HideInInspector]
    public int runCount = 0;

    /* LOD list, updated over time as new SOBJs are loaded */
    [HideInInspector]
    private List<LOD> LODList = new List<LOD>();

    /* all found SOBJ files */
    private List<List<string>> filePaths = new List<List<string>>();

    /* all objects created from SOBJ files */
    private List<GameObject> terrainObjects = new List<GameObject>();

    /// <summary>
    /// dictionary storing indices of all loaded terrainObjects
    /// </summary>
    private Dictionary<string, List<int>> terrainObjectsNameDictionary = new Dictionary<string, List<int>>();

    /* queue of ThreadedOBJLoaders to be enabled */
    public Queue<ThreadedOBJLoader> threadedOBJLoadersHighPriority = new Queue<ThreadedOBJLoader>();
    public Queue<ThreadedOBJLoader> threadedOBJLoadersLowPriority  = new Queue<ThreadedOBJLoader>();

    /* timer */
    private System.Diagnostics.Stopwatch timer;

    /* Legacy loading */
    private bool usingLegacyLoading = true;

    /* StreamedTerrainData */
    public StreamedTerrainData terrainData;

    /**
    * Awake is called once during initialisation
    */
    void Awake()
    {
        if (autoRun)
        {
            if (importFolder != "")
            {
                importLocation = importFolder;
            }

            Run();
        }

        if (hideBoxOnAwaken)
        {
            gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    /**
    * Add loaded renderer component to LOD component
    */
    void applyLODs()
    {
        List<LOD> newLODList = new List<LOD>();

        for (int i = 0; i < LODList.Count; i++)
        {
            newLODList.Add(new LOD(1.0F / ((float)i + 1.5f), LODList[i].renderers));
        }

        lodGroup.SetLODs(newLODList.ToArray());
        lodGroup.RecalculateBounds();
    }

    public int CalculateAvailableCores()
    {
        int loopsToDo = numberOfThreads;
        if (numberOfThreads == -1)
        {
            loopsToDo = Math.Max(1, (Environment.ProcessorCount / 2) - 2); // assumes hyper threading, min 1
        }
        return loopsToDo;
    }

    /**
    * Raycast up and down into the scene and place the player on the loaded scenery
    */
    void PlacePlayer()
    {
        if (player != null && playerPlaced == false)
        {
            bool tempBackFace = Physics.queriesHitBackfaces;
            Physics.queriesHitBackfaces = true;

            Vector3 castOrigin = gameObject.transform.position;
            RaycastHit hit;

            if (Physics.Raycast(castOrigin, Vector3.down, out hit, 5000))
            {
                player.transform.position = hit.point + Vector3.up;
                playerPlaced = true;
            }
            else if (Physics.Raycast(castOrigin, Vector3.up, out hit, 5000))
            {
                player.transform.position = hit.point + Vector3.up;
                playerPlaced = true;
            }
            else
            {
                player.transform.position = gameObject.transform.position + Vector3.up;
            }

            //disable collision on self
            Collider collider = gameObject.GetComponent<Collider>();
            collider.enabled = false;

            Physics.queriesHitBackfaces = tempBackFace;
        }
    }

    /**
    * Called when a ThreadedOBJLoader has finished loading its SOBJ file
    * The gameObject will have been created at this point
    * 
    * @param loader The ThreadedOBJLoader which has completed it's load
    */
    void OBJLoaded(ThreadedOBJLoader loader)
    {
        // if for some reason we've got a loader that should have been deleted
        if (loader.ShouldDelete)
        {
            loader.DestroyLoader();
            return;
        }

        string name = Path.GetFileName(loader.filePath);
        terrainObjectsNameDictionary[name] = new List<int>();

        // add newly created mesh objects to list of created objects
        foreach (GameObject meshGameObject in loader.meshObjects)
        {
            terrainObjectsNameDictionary[name].Add(terrainObjects.Count);
            terrainObjects.Add(meshGameObject);

        }
        foreach (GameObject colliderGameObject in loader.colliderObjects)
        {
            terrainObjectsNameDictionary[name].Add(terrainObjects.Count);
            terrainObjects.Add(colliderGameObject);
        }
    }

    /**
    * Called when a ThreadedOBJLoader has finished loading its SOBJ file
    * The gameObject will have been created at this point
    * 
    * @param loader The ThreadedOBJLoader which has completed it's load
    */
    void OBJLoaded_Legacy(ThreadedOBJLoader loader)
    {
        OBJLoaded(loader);

        // if more objloaders to run
        // run them 
        if (threadedOBJLoadersHighPriority.Count != 0)
        {
            ThreadedOBJLoader objLoader = threadedOBJLoadersHighPriority.Dequeue();
            objLoader.enabled = true;
            objLoader.Run();
        }
        else //complete
        {
            Debug.Log("Total Elapsed Loading time " + timer.ElapsedMilliseconds + "ms");
            if (LoadingIcon != null)
            {
                LoadingIcon.SetActive(false);
            }
        }

        // if the new object is part of a new lod group        
        if (loader.lodLevel >= LODList.Count)
        {
            applyLODs();

            // create a new group to fill
            List<Renderer> newRenderers = new List<Renderer>();

            //the new LOD is inserted at the beginning of the List
            //
            // Unity lods go High -> Low 
            // So this List must be sorted that way
            //
            LODList.Insert(0, new LOD(0, newRenderers.ToArray()));

            //attempt to place player object in the scene
            PlacePlayer();
        }

        //get a list renderers from the current highest LOD list
        List<Renderer> renderers = new List<Renderer>(LODList[0].renderers); // 0 -> highest detail LOD

        //add new renderers to the LOD List
        foreach (GameObject meshObject in loader.meshObjects)
        {
            renderers.AddRange(meshObject.GetComponentsInChildren<Renderer>());
        }

        // overwrite the old LOD list with new one
        LODList[0] = new LOD(0, renderers.ToArray());

        // if this is the last loader - apply LODS
        ThreadedOBJLoader[] objLoaders = gameObject.GetComponents<ThreadedOBJLoader>();
        if (objLoaders.Length == 1)
        {
            applyLODs();
        }
    }

    /// <summary>
    /// The legacy loader creates a queue of obj loaders
    /// 
    /// threads then work through this queue loading objs as they are available
    /// 
    /// </summary>
    private void DoRun_Legacy()
    {
        // setup LOD group
        if (lodGroup)
        {
            lodGroup.SetLODs(LODList.ToArray()); //reset if exists
        }
        else
        {
            lodGroup = gameObject.AddComponent<LODGroup>();
        }

        List<string> files = null;

        // find files
        files = new List<string>(Directory.GetFiles(importLocation, "*.SOBJ"));
        Debug.Log("num:" + files.Count);

        // search for SOBJ within sub directories also          
        string[] directories = Directory.GetDirectories(importLocation);

        for (int i = 0; i < directories.Length; i++)
        {
            files.AddRange(Directory.GetFiles(directories[i], "*.SOBJ"));
            Debug.Log("num:" + files.Count);
        }

        // sort alphabetically
        files.Sort();

        char currentPrefix = ' ';
        int currentLODLevel = -1;

        // loop over files
        // add file names to appropriate array
        for (int i = 0; i < files.Count; i++)
        {
            string name = Path.GetFileNameWithoutExtension(files[i]);
            char prefix = name[0];

            if (prefix != currentPrefix)
            {
                currentPrefix = prefix;
                currentLODLevel += 1;

                filePaths.Add(new List<string>());
            }

            filePaths[currentLODLevel].Add(files[i]);
        }

        // Create all appropriate objloaders
        int LODIndex = 0;

        // create set of ThreadedOBJLoaders for each found LOD level
        foreach (List<string> LODlevel in filePaths)
        {
            int modelIndex = 0;

            // create ThreadedOBJLoader for each SOBJ file
            foreach (string LODFilePath in LODlevel)
            {
                ThreadedOBJLoader objLoader = gameObject.AddComponent<ThreadedOBJLoader>();

                objLoader.OBJLoadedEvent += this.OBJLoaded_Legacy;

                objLoader.filePath = LODFilePath;
                objLoader.lodLevel = LODIndex;
                objLoader.modelIndex = modelIndex;
                objLoader.blockOnLoad = blockOnLoad;

                if (onlyGenerateSimpleColliders)
                {
                    objLoader.generateColliders = (LODIndex == 0) ? generateColliders : false;
                }
                else
                {
                    objLoader.generateColliders = generateColliders;
                }

                objLoader.enabled = false;

                // add to queue
                threadedOBJLoadersHighPriority.Enqueue(objLoader);
                modelIndex++;
            }
            LODIndex++;
        }

        // for #loopsToDo, enable and run objloaders
        for (int i = 0; i < Math.Min(CalculateAvailableCores(), threadedOBJLoadersHighPriority.Count); i++)
        {
            ThreadedOBJLoader objLoader = threadedOBJLoadersHighPriority.Dequeue(); // remove activated loaders from the queue
            objLoader.enabled = true;
            objLoader.Run();
        }
    }

    /**
    * Run loading of terrain, this will load from the folder at importLocation
    */
    public void Run()
    {
        timer = System.Diagnostics.Stopwatch.StartNew();

        playerPlaced = false;

        // only 1 scene loaded at a time
        ClearAllLoadedOBJS();
        if (LoadingIcon != null)
        {
            LoadingIcon.SetActive(true);
        }

        // initialise import
        if (importLocation == "")
        {
            var directory = new DirectoryInfo(Application.dataPath);
            importLocation = Path.Combine(directory.Parent.FullName, "Assets\\Model");
        }
        Debug.Log("location:" + importLocation);


        // load terrain meta data
        terrainData = StreamedTerrainData.LoadStreamedTerrainData(importLocation);

        // position player
        if (playerStart.Equals(new Vector3(-999999, -999999, -999999)))
        {
            Debug.Log(terrainData.startPosition);
            player.transform.position = terrainData.startPosition;
            gameObject.transform.position = terrainData.startPosition;
        }
        else
        {
            player.transform.position = playerStart;
            gameObject.transform.position = playerStart;
        }

        // start run
        if (terrainData.IsUsingLegacyLoading())
        {
            Debug.Log("legacy?");
            DoRun_Legacy();
        }
        else
        {
            StartCoroutine(this.DoRun_Coroutine());
        }
    }

    /// <summary>
    /// Removes all previously loaded data and destroys GameObjects
    /// </summary>
    public void ClearAllLoadedOBJS()
    {
        // stop running coroutines
        StopAllCoroutines();

        LODList.Clear();
        filePaths.Clear();

        // for each loader
        foreach (ThreadedOBJLoader ObjectLoader in transform.GetComponents<ThreadedOBJLoader>())
        {
            ObjectLoader.enabled = true;
            ObjectLoader.DestroyLoader();
        }

        threadedOBJLoadersHighPriority.Clear();
        threadedOBJLoadersLowPriority.Clear();

        foreach (GameObject terrainObject in terrainObjects)
        {
            if (terrainObject.GetComponent<MeshRenderer>())
            {
                Destroy(terrainObject.GetComponent<MeshRenderer>().material.mainTexture);
                Destroy(terrainObject.GetComponent<MeshRenderer>().material);
            };

            if (terrainObject.GetComponent<MeshFilter>())
            {
                Destroy(terrainObject.GetComponent<MeshFilter>().sharedMesh);
            };

            if (terrainObject.GetComponent<MeshCollider>())
            {
                Destroy(terrainObject.GetComponent<MeshCollider>());
            };

            Destroy(terrainObject);
        }
    }
       
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IEnumerator DoRun_Coroutine()
    {
        int coreCount = CalculateAvailableCores();

        DistanceBasedTerrainLoader distanceBasedTerrainLoader = gameObject.AddComponent<DistanceBasedTerrainLoader>();

        yield return new WaitForSeconds(0.005f); //stagger the coroutines

        DistanceBasedTerrainLoader distanceBasedTerrainLoaderLow    = gameObject.AddComponent<DistanceBasedTerrainLoader>();
        distanceBasedTerrainLoaderLow.isLow = true;

        distanceBasedTerrainLoader.sisterLoader = distanceBasedTerrainLoaderLow;
        distanceBasedTerrainLoaderLow.sisterLoader = distanceBasedTerrainLoader;

        distanceBasedTerrainLoader.Run();
        distanceBasedTerrainLoaderLow.Run();

        bool shouldLoop = true;
        while (shouldLoop)
        {      
            if (runCount < coreCount)
            {
                for (int i = 0; i < Math.Min(coreCount - runCount, threadedOBJLoadersHighPriority.Count); i++)
                {
                    ThreadedOBJLoader objLoader = threadedOBJLoadersHighPriority.Dequeue(); // remove activated loaders from the queue
                    objLoader.enabled = true;
                    objLoader.Run();

                    runCount++;
                }

                for (int i = 0; i < Math.Min(coreCount - runCount, threadedOBJLoadersLowPriority.Count); i++)
                {
                    ThreadedOBJLoader objLoader = threadedOBJLoadersLowPriority.Dequeue(); // remove activated loaders from the queue
                    objLoader.enabled = true;
                    objLoader.Run();

                    runCount++;
                }
            }

            yield return new WaitForSeconds(0.01f);
        }
    }
}