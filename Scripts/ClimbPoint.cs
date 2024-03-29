using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClimbPoint : MonoBehaviour
{
   [SerializeField] List<Neighbour> neighbors;
   [SerializeField] bool mountPoint;
   

    private void Awake()
    {
        var twoWayNeighbours = neighbors.Where(n => n.isTwoWay);

        foreach(var neighbour in neighbors.Where(n => n.isTwoWay))
        {
            neighbour.point?.CreateConnection(this, -neighbour.direction, neighbour.connectionType, neighbour.isTwoWay);

        }
    }

    public void CreateConnection(ClimbPoint point, Vector2 direction, ConnectionType connectionType, bool isTwoWay = true)
    {
        var neighbour = new Neighbour
        {
            point = point,
            direction = direction,
            connectionType = connectionType,
            isTwoWay = isTwoWay
           
        };
        neighbors.Add(neighbour);   
    }
    public Neighbour GetNeighbour(Vector2 direction)
    {
        Neighbour neighbour = null;

        if(direction.y != 0)
            neighbour = neighbors.FirstOrDefault(n => n.direction.y == direction.y);
        
        if(neighbour == null && direction.x != 0)
            neighbour = neighbors.FirstOrDefault(n => n.direction.x == direction.x);
        

        return neighbour;
    }

    private void OnDrawGizmos()
    {
        foreach(var neighbour in neighbors)
        {
            if(neighbour.point != null)
                Debug.DrawLine(transform.position, neighbour.point.transform.position, (neighbour.isTwoWay)? Color.blue : Color.gray);
        }
    }
    public bool MountPoint => mountPoint;

}
[System.Serializable]
public class Neighbour
{
    public ClimbPoint point;
    public Vector2 direction;
    public ConnectionType connectionType;
    public bool isTwoWay = true;
}
public enum ConnectionType { Jump, Move}