using System.Collections.Generic;
using UnityEngine;

public class StarNode : MonoBehaviour
{
    public List<StarNode> validConnections = new List<StarNode>();

    public bool IsConnectedTo(StarNode other)
    {
        return validConnections.Contains(other);
    }
}