/******************************************************************************
*
* Name:        TerrainLoader.cs
* Project:     3dTeLC
* Author:      Martin Kearl 2019
* Institution: University of Portsmouth, UK
*
******************************************************************************
* Copyright (c) 2016-2020
* license notice to be determined 
*****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DistanceBasedTerrainLoader : MonoBehaviour
{
    public TerrainLoader terrainLoader      = null;
    public StreamedTerrainData terrainData  = null;
    public DistanceBasedTerrainLoader sisterLoader = null;

    public bool isLow       = false;
    public bool shouldLoop  = true;

    private List<GameObject>                terrainObjects = new List<GameObject>();
    private Dictionary<string, List<int>>   terrainObjectsNameDictionary = new Dictionary<string, List<int>>();
    
    static int modelIndex = 0;
    
    public void Run()
    {
        terrainLoader = GetComponent<TerrainLoader>();

        if (terrainLoader != null)
        {
            terrainData = new StreamedTerrainData();

            if (isLow)
            {
                terrainData.LoadSceneXML(TerrainLoader.importLocation, "sceneLow.xml");
            }
            else
            {
                terrainData.LoadSceneXML(TerrainLoader.importLocation);
            }  
        }

        if (terrainData != null)
        {
            StartCoroutine(this.Run_Coroutine());
        }
        else
        {
            Destroy(this);
        }
    }

    /*
    void OBJLoaded(ThreadedOBJLoader loader, int j)
    {
        // if for some reason we've got a loader that should have been deleted
        if (loader.ShouldDelete)
        {
            loader.DestroyLoader();
            return;
        }

        //string name = Path.GetFileName(loader.filePath);

        string name = terrainData.CellList[j];
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
    */

    /// <summary>
    /// Removes all previously loaded data and destroys GameObjects
    /// </summary>
    public void ClearAllLoadedOBJS()
    {
        // stop running coroutines
        StopAllCoroutines();

        // for each loader
        foreach (ThreadedOBJLoader ObjectLoader in transform.GetComponents<ThreadedOBJLoader>())
        {
            ObjectLoader.enabled = true;
            ObjectLoader.DestroyLoader();
        }

        //threadedOBJLoadersToRun.Clear();

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
    /// Removes all previously loaded data and destroys GameObjects
    /// </summary>
    public bool CheckCellExists(string cellHash)
    {
        if (cellHash != null)
        {
            return terrainObjectsNameDictionary.ContainsKey(cellHash);
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IEnumerator Run_Coroutine()
    {
        while (shouldLoop)
        {
            for (int i = 0; i < terrainData.CellList.Count(); i++)
            {
                // get data
                StreamedTerrainCellData terrainCellData = terrainData.CellList[i];

                // calculate manhattan distance

                float modifiedCellPositionX = terrainCellData.Position.x - WorldRebaser.accumulatedOffset.x;
                float modifiedCellPositionZ = terrainCellData.Position.z - WorldRebaser.accumulatedOffset.z;
                double manhattanDistance = Math.Abs(modifiedCellPositionX - PositionSingleton.playerContinousPosition.x);
                manhattanDistance = manhattanDistance + Math.Abs(modifiedCellPositionZ - PositionSingleton.playerContinousPosition.z);

                // check distance in grid space
                // if low LOD check if far, else check close
                if (isLow ? manhattanDistance / terrainData.gridSize > terrainLoader.loadedGridDistance :
                            manhattanDistance / terrainData.gridSize <= terrainLoader.loadedGridDistance)
                {
                    if (terrainCellData.isLoaded == false && terrainCellData.isLoading == false)
                    {
                        // new loader
                        ThreadedOBJLoader objLoader = gameObject.AddComponent<ThreadedOBJLoader>();

                        // on complete callback lambda
                        int j = i;
                        objLoader.OBJLoadedEvent += (ThreadedOBJLoader loader) =>
                        {
                                //this.OBJLoaded(loader, j);

                                if (loader.ShouldDelete)
                                {
                                    loader.DestroyLoader();
                                    return;
                                }

                                //string name = Path.GetFileName(loader.filePath);

                                string hash = terrainCellData.Hash;
                                terrainObjectsNameDictionary[hash] = new List<int>();

                                // add newly created mesh objects to list of created objects
                                foreach (GameObject meshGameObject in loader.meshObjects)
                                {
                                    terrainObjectsNameDictionary[hash].Add(terrainObjects.Count);
                                    terrainObjects.Add(meshGameObject);

                                }
                                foreach (GameObject colliderGameObject in loader.colliderObjects)
                                {
                                    terrainObjectsNameDictionary[hash].Add(terrainObjects.Count);
                                    terrainObjects.Add(colliderGameObject);
                                }

                                terrainData.SetLoaded(j, true);
                                terrainData.SetLoading(j, false);
                                terrainLoader.runCount--;
                        };

                        // setup
                        objLoader.filePath          = terrainCellData.Path;
                        objLoader.modelIndex        = modelIndex++;
                        objLoader.blockOnLoad       = false;
                        objLoader.generateColliders = !isLow;
                        objLoader.enabled           = false;

                        // set to loading status and place loader in queue
                        terrainData.SetLoading(j, true);
                        if (isLow)
                        {
                            terrainLoader.threadedOBJLoadersLowPriority.Enqueue(objLoader);
                        }
                        else
                        {
                            terrainLoader.threadedOBJLoadersHighPriority.Enqueue(objLoader);
                        }
                    }
                }

                // outside of range and is loaded?
                else if (terrainCellData.isLoaded && sisterLoader.CheckCellExists(terrainCellData.Hash)) // check other distance based loader here
                {
                    foreach (int index in terrainObjectsNameDictionary[terrainCellData.Hash])
                    {
                        // destroy game object
                        GameObject terrainObject = terrainObjects[index];

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
                    terrainObjectsNameDictionary[terrainCellData.Hash].Clear();
                    terrainObjectsNameDictionary.Remove(terrainCellData.Hash);

                    // mark as unloaded
                    terrainData.SetLoaded(i, false);
                }
            }
            yield return new WaitForSeconds(0.01f);
        }
    }
}
