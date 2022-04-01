/******************************************************************************
 *
 * Name:        StreamedObjData.cs
 * Project:     3dTeLC
 * Author:      Martin Kearl 2019
 * Institution: University of Portsmouth, UK
 *
 ******************************************************************************
 * Copyright (c) 2016-2020
 * license notice to be determined 
 *****************************************************************************/
using System.Collections.Generic;
using UnityEngine;

/**
* Container class for loaded SOBJ data
*/
public class StreamedObjData {
    /* xyz position of mesh centre */
    public Vector3 translation;

    /* xyz position information for mesh */
    public List<Vector3> positions;

    /* uv texture coordinate information for mesh */
    public List<Vector2> texcoords;

    /* triangle index information for mesh, every 3 indices represents 1 triangle */
    public List<int> triangles;

    /* name for streamed data, derived from file name */
    public string name = "Mesh";
    
    /* absolute path to texture for this mesh */
    public string texturePath;

    /* Width of associated texture */
    public int textureWidth;

    /* Height of associated texture */
    public int textureHeight;

    /* Loaded texture data in DDS format with header removed */
    public byte[] textureCompressedByteArray;

    /* should collision information be loaded alongside visual mesh */
    public bool generateCollider = true;

    /* List of xyz position information for physics meshes */
    public List<List<Vector3>> physPositions;

    /* List of triangle index information information for physics meshes, every 3 indices represents 1 triangle */
    public List<List<int>> physTriangles;
    
    /*
    * Constructor, initialises all Lists 
    */
    public StreamedObjData()
    {
        positions = new List<Vector3>();
        texcoords = new List<Vector2>();
        triangles = new List<int>();

        physPositions = new List<List<Vector3>>();
        physTriangles = new List<List<int>>();

        // add empty defaults
        physPositions.Add(new List<Vector3>());
        physTriangles.Add(new List<int>());
    }
}
