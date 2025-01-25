// Copyright © 2025 Always Active Technologies PTY Ltd

using System.Xml.Serialization;

namespace TechAptV1.Client.Models;

[XmlType(Namespace = "Number")]
public class Number
{
    [XmlAttribute(Namespace = "Value")]
    public int Value { get; set; }
    [XmlAttribute(Namespace = "IsPrime")]
    public bool IsPrime { get; set; }
}
