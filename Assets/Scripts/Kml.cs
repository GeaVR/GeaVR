using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

[XmlRoot("kml")]
public class Kml  {


    [XmlElement("Document")]
    public List<Document> document { get; set; }
}

public class Document
{
    public string Name { get; set; }
}