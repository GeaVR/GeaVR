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
