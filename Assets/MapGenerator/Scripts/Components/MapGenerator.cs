using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static RoomTileMap;
public class MapGenerator : MonoBehaviour
{
    public RoomTileMap tileSetData;
    public bool generateOnStart = true;

    private List<DoorLocation> mgOpenDoors = new List<DoorLocation>();
    private List<DoorLocation> mgUnlockedDoors = new List<DoorLocation>();
    private List<DoorLocation> mgLockedDoors = new List<DoorLocation>();
    private List<PlacedRoomData> mgPlacedRooms = new List<PlacedRoomData>();

    public struct DoorLocation
    {
        public Vector2 position;
        public float rotation;

        public int roomIndex;
    }

    public struct PlacedRoomData
    {
        public GameObject roomObject;
        public Vector2 position;
        public Vector2 scale;
        public float rotation;
    }

    void Start() 
    {
        if(generateOnStart)
        {
            GenerateMap(tileSetData, out mgOpenDoors, out mgUnlockedDoors, out mgLockedDoors, out mgPlacedRooms, true);
        }

    }

    /// <summary>
    /// Attempts to generate a map based off of the provided tile set
    /// </summary>
    ///<returns>
    /// Returns true when the map was fully generated, returns false otherwise.
    ///</returns>
    public bool GenerateMap(RoomTileMap tileSet, 
                            out List<DoorLocation> openDoors,
                            out List<DoorLocation> unlockedDoors,
                            out List<DoorLocation> lockedDoors,
                            out List<PlacedRoomData> placedRooms,
                            bool enableDebug = false)
    {
        openDoors = new List<DoorLocation>(); //list of all doors that have not had an attempted tile placed at the door
        unlockedDoors = new List<DoorLocation>(); //list of all doors that have a tile on both sides
        lockedDoors = new List<DoorLocation>(); //list of all doors that lead nowhere
        placedRooms = new List<PlacedRoomData>(); //list of all placed rooms

        #region check if data is useable
        bool safeToStart = true;
        if(tileSet.startTiles.Count == 0)
        {
            Debug.LogError("You must set at least one starting tile before attempting to generate a map");
            safeToStart = false;
        }
        if(tileSet.endTiles.Count == 0)
        {
            Debug.LogError("You must set at least one ending tile before attempting to generate a map");
            safeToStart = false;
        }
        if(tileSet.doorCaps.Count == 0)
        {
            Debug.LogError("You must set at least one door capping tile before attempting to generate a map");
            safeToStart = false;
        }
        if(!safeToStart)
        {
            return false;
        }
        #endregion
        int maxAttempts = 5;
        bool endingMade = false;
        while((!endingMade || placedRooms.Count < tileSet.minTileCount) && maxAttempts > 0)
        {
            maxAttempts--;

            //reset data for just incase this is not the first time around
            foreach(PlacedRoomData prd in placedRooms)
            {
                Destroy(prd.roomObject);
            }
            endingMade = false;
            openDoors.Clear();
            unlockedDoors.Clear();
            lockedDoors.Clear();
            placedRooms.Clear();
            //place starting tile
            TileWithState startingTileData = tileSet.startTiles[Random.Range(0,tileSet.startTiles.Count)];
            float rotation = tileSet.allowRotation ? 90 * Random.Range(0,4) : 0;
            
            PlaceRoom(startingTileData.tileObject, new Vector2(0,0), rotation, tileSet.axisMode, -1, out List<DoorLocation> newRooms, out PlacedRoomData newRoomData, 0, tileSet.useRB2D);
            placedRooms.Add(newRoomData);
            openDoors.AddRange(newRooms);
            
            //the maximum amount of tiles placed
            int tempStop = tileSet.maxTileCount;
            
            while(openDoors.Count > 0)
            {
                
                if(GenerateRoom(0, openDoors, unlockedDoors, lockedDoors, placedRooms, tileSet, tileSet.minTileCount < placedRooms.Count && !endingMade, out bool usedExit))
                {
                    if(usedExit)
                    {
                        endingMade = true;
                    }
                    unlockedDoors.Add(openDoors[0]);
                    openDoors.RemoveAt(0);
                    tempStop -= 1;
                }
                else
                {
                    lockedDoors.Add(openDoors[0]);
                    openDoors.RemoveAt(0);
                }
                if(tempStop <= 0)
                {
                    break;
                }
            }
        }
        //if we still dont have enough rooms we give up
        if(placedRooms.Count < tileSet.minTileCount || !endingMade)
        {
            Debug.LogError("Something prevented a map from being fully generated. This can be caused by not enough tile varients, poorly weighted tiles, or a pre existing object is blocking collision.");
            return false;
        }

        openDoors.AddRange(lockedDoors);
        lockedDoors.Clear();

        //combine doors that ere in the same place
        while(openDoors.Count > 1)
        {
            bool foundOne = false;
            for(int i = 1; i < openDoors.Count; i++)
            {
                if(Vector2.Distance(openDoors[0].position, openDoors[i].position) < 0.1f)
                {
                    if(AngleDiff(openDoors[0].rotation, openDoors[i].rotation) > 0.1f)
                    {
                        unlockedDoors.Add(openDoors[0]);
                        openDoors.RemoveAt(i);
                        openDoors.RemoveAt(0);
                        foundOne = true;
                        break;
                    }
                }
            }
            if(!foundOne)
            {
                lockedDoors.Add(openDoors[0]);
                openDoors.RemoveAt(0);
            }
        }


        //add any remaining doors to the locked door catagory
        lockedDoors.AddRange(openDoors);
        openDoors.Clear();



        //add locked doors to doorways that do not lead to anything
        float[] doorProbabilities = new float[tileSet.doorCaps.Count];
        for(int i = 0; i < doorProbabilities.Length; i++)
        {
            doorProbabilities[i] = tileSet.doorCaps[i].rarity;
        }
        foreach(DoorLocation dl in lockedDoors)
        {
            int doorIndex = SkewedNum(doorProbabilities);
            GameObject newDoorFab = tileSet.doorCaps[doorIndex].tileObject;
            PlaceRoom(newDoorFab, dl.position, dl.rotation, tileSet.axisMode, 0, out List<DoorLocation> newOpenDoors, out PlacedRoomData newDoor, 0, tileSet.useRB2D);
        }




        return false;
    }


    /// <summary>
    /// picks a room at random untill a room fits
    /// </summary>
    /// <returns>
    /// true a room was placed
    /// </returns>
    private bool GenerateRoom(int doorIndex, 
                              List<DoorLocation> openDoors, 
                              List<DoorLocation> unlockedDoors,
                              List<DoorLocation> lockedDoors, 
                              List<PlacedRoomData> placedRooms, 
                              RoomTileMap tileSet, 
                              bool allowEndingRooms, out bool usedEndingRoom)
    {
        usedEndingRoom = false;
        Vector2 targetDoorPos = openDoors[doorIndex].position;
        //add endTiles to probability if allowEndingRooms is true
        float[] roomProbabilities = new float[allowEndingRooms ? tileSet.tileSet.Count + tileSet.endTiles.Count : tileSet.tileSet.Count];
        for(int i = 0; i < roomProbabilities.Length; i++)
        {
            if(i >= tileSet.tileSet.Count)
            {
                roomProbabilities[i] = tileSet.endTiles[i-tileSet.tileSet.Count].rarity;
            }
            else
            {
                roomProbabilities[i] = tileSet.tileSet[i].rarity;
            }
        }
        bool roomFits = false;
        while(!roomFits)
        {
            int roomIndex = SkewedNum(roomProbabilities);
            if(roomIndex == -1)
            {
                return false;
            }
            roomProbabilities[roomIndex] = 0;
            
            //if the tile is over the tileSet size then it is in endTiles
            TileWithState tws = roomIndex < tileSet.tileSet.Count ? tileSet.tileSet[roomIndex] : tileSet.endTiles[roomIndex-tileSet.tileSet.Count];

            roomFits = CheckRoomFits(openDoors[doorIndex], tws, placedRooms, out Vector2 position, 
                          out float rotation, out PlacedRoomData newRoom, out List<DoorLocation> newDoors, 
                          tileSet, openDoors[doorIndex].roomIndex, out GameObject placedRoom);
            if(!roomFits)
            {
                if(placedRoom)
                {
                    Destroy(placedRoom); //destroy the room if it was left created and should not be used
                }
            }
            else
            {
                placedRooms.Add(newRoom);
                openDoors.AddRange(newDoors);
                usedEndingRoom = roomIndex >= tileSet.tileSet.Count;
            }
            

        }
        return true;


    }

    /// <summary>
    /// Spawns a room and checks the collision to make sure it can fit
    /// </summary>
    /// <returns>
    /// true if the room fits
    /// </returns>
    private bool CheckRoomFits(DoorLocation targetDoor, TileWithState tile, List<PlacedRoomData> placedRooms, out Vector2 position, 
                               out float rotation, out PlacedRoomData newRoom, out List<DoorLocation> newDoors, RoomTileMap tileSet, int roomIndex, out GameObject placedRoom)
    {
        newRoom = new PlacedRoomData();
        newDoors = new List<DoorLocation>();
        placedRoom = null;

        List<TileData.DoorValues> doors = tile.tileObject.GetComponent<TileData>().doorPositions;//get list of doors off of the tile

        //give them all a equal probability of being picked
        float[] posProbabilities = new float[doors.Count];
        for(int i = 0; i < posProbabilities.Length; i++)
        {
            posProbabilities[i] = 1;
        }

        bool validResult = false;
        while(!validResult)
        {
            //pick door index at random
            int doorIndex = SkewedNum(posProbabilities);
            if(doorIndex == -1)
            {
                //return false if we run out of doors to try
                position = new Vector2(0,0);
                rotation = 0;
                return false;
            }
            DoorLocation doorPosition = new DoorLocation() { position = doors[doorIndex].position, rotation = doors[doorIndex].rotation };
            posProbabilities[doorIndex] = 0; //remove this doorIndex from the running for the next loop through

            rotation = 0;
            float doorRot = doorPosition.rotation;

            

            if(tileSet.allowRotation)
            {
                //find the rotation by matching the rotation of the cur door to the 180 degree ooposite of target door
                float targetRot = DegWrap(targetDoor.rotation + 180);
                rotation = targetRot - DegWrap(doorRot);
            }
            else
            {
                float targetRot = DegWrap(targetDoor.rotation + 180);
                if(!(AngleDiff(targetRot, doorRot) < 1f))
                {
                    continue;
                }
            }

            //move the position to the correct place
            if(tile.tileObject.GetComponent<TileData>().colliders3D.Count > 0)
            {
                Vector3 rotDoorPosV3 = Quaternion.Euler(0,rotation,0) * new Vector3(doorPosition.position.x, 0, doorPosition.position.y);
                position = targetDoor.position - new Vector2(rotDoorPosV3.x,rotDoorPosV3.z); 
            }
            else
            {
                Vector3 rotDoorPosV3 = Quaternion.Euler(0,0,rotation) * new Vector3(doorPosition.position.x, doorPosition.position.y, 0);
                position = targetDoor.position - new Vector2(rotDoorPosV3.x,rotDoorPosV3.y);  
            }
            // Vector3 rotDoorPosV3 = Quaternion.Euler(0,0,rotation) * new Vector3(doorPosition.position.x, doorPosition.position.y, 0);
            // position = targetDoor.position - new Vector2(rotDoorPosV3.x,rotDoorPosV3.y);  
            

            //do collision check here
            placedRoom = PlaceRoom(tile.tileObject, position, rotation, tileSet.axisMode, doorIndex, out List<DoorLocation> newOpenDoors, out PlacedRoomData newRoomData, roomIndex, tileSet.useRB2D);
            newDoors.AddRange(newOpenDoors);
            newRoom = newRoomData;
            TileData td = placedRoom.GetComponent<TileData>();
            if(td.colliders2D.Count > 0)
            {
                //do 2d collision checks
                foreach(Collider2D c in td.colliders2D)
                {
                    List<Collider2D> results = new List<Collider2D>();
                    c.OverlapCollider(new ContactFilter2D(), results);
                    if(results.Count > 0)
                    {
                        //check if the any of the overlapping colliders are on a diferent object, if so then do not spawn the tile
                        //this is slow af and could use an optimization pass
                        foreach(Collider2D c2 in results)
                        {
                            if(!td.colliders2D.Contains(c2))
                            {
                                if(Physics2D.Distance(c,c2).distance < -tileSet.maxOverlap)
                                {
                                    return false;
                                }
                            }
                        }
                        
                    }
                }
                return true;
            }
            else 
            {
                //do 3d collision checks
                foreach(Collider c in td.colliders3D)
                {
                    List<(Collider, float)> results = GetCollisions(c, placedRooms);
                    if(results.Count > 0)
                    {
                        foreach((Collider, float) c2 in results)
                        {
                            if(!td.colliders3D.Contains(c2.Item1))
                            {
                                if(Mathf.Abs(c2.Item2) > tileSet.maxOverlap)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
                return true;
            }
        }

        //return false
        position = new Vector2(0,0);
        rotation = 0;
        return false;
    }

    /// <summary>
    /// Places a room into the world at a set position and rotation.
    /// </summary>
    private GameObject PlaceRoom(GameObject room, Vector2 position, float rotation, AxisMode axisMode, 
                                 int ignoreDoorIndex, out List<DoorLocation> newOpenDoors, out PlacedRoomData placedRoom, int roomIndex, bool useRB2D)
    {
        GameObject tileObject = Instantiate(room, transform);
        switch(axisMode)
        {
            case AxisMode.XY:
                tileObject.transform.position = new Vector3(position.x, position.y, 0);
                tileObject.transform.eulerAngles = new Vector3(0, 0, rotation);
                break;
            case AxisMode.XZ:
                tileObject.transform.position = new Vector3(position.x, 0, position.y);
                tileObject.transform.eulerAngles = new Vector3(0, rotation, 0);
                break;
        }
        Physics2D.SyncTransforms();
        Physics.SyncTransforms();
        TileData td = tileObject.GetComponent<TileData>();
        td.gizmoAxisMode = axisMode;

        //add new doors to the openDoors list
        newOpenDoors = new List<DoorLocation>();
        for(int i = 0; i < td.doorPositions.Count; i++)
        {
            if(i != ignoreDoorIndex)
            {
                Vector3 rotDoorPosV3;
                Vector3 mapSpaceDoorPos;
                if(room.GetComponent<TileData>().colliders3D.Count > 0)
                {
                    //door pos for xz
                    rotDoorPosV3 = Quaternion.Euler(0,0,rotation) * new Vector3(-td.doorPositions[i].position.x, td.doorPositions[i].position.y, 0);
                    mapSpaceDoorPos = new Vector2(-rotDoorPosV3.x,rotDoorPosV3.y) + position;
                }
                else
                {
                    //door pos for xy
                    rotDoorPosV3 = Quaternion.Euler(0,0,rotation) * new Vector3(td.doorPositions[i].position.x, td.doorPositions[i].position.y, 0);
                    mapSpaceDoorPos = new Vector2(rotDoorPosV3.x,rotDoorPosV3.y) + position;
                }
                newOpenDoors.Add(new DoorLocation(){ position = mapSpaceDoorPos, rotation = DegWrap(td.doorPositions[i].rotation + rotation), roomIndex = roomIndex });
            }
        }
        placedRoom = new PlacedRoomData(){ position = position, rotation = rotation, scale = td.tileSize, roomObject = tileObject };
        return tileObject;
    }

    /// <summary>
    /// Randomly picks an index based off the weights in the indexes
    /// </summary>
    private int SkewedNum(float[] probabilities)
    {
        float total = 0;
        for(int i = 0; i < probabilities.Length; i++)
            total += probabilities[i];
        if(total == 0)
        {
            return -1;
        }
        float target = total * Random.value;

        int index = 0;
        float curVal = 0;
        while(index < probabilities.Length - 1)
        {
            curVal += probabilities[index];
            if(curVal > target)
            {
                return index;
            }
            index++;
        }
        //for just incase floating points cause a weird value
        if(index >= probabilities.Length)
        {
            index = probabilities.Length - 1;
        }
        return index;
    }

    /// <summary>
    /// Wraps a degree value to be inbetween 0(inclusive) and 360(exclusive)
    /// </summary>
    private float DegWrap(float original)
    {
        while(original >= 360)
        {
            original -= 360;
        }
        while(original < 0)
        {
            original += 360;
        }
        return original;
    }

    private void OnDrawGizmos()
    {
        foreach(DoorLocation d in mgLockedDoors)
        {
            Gizmos.color = Color.magenta;
            switch(tileSetData.axisMode)
            {
                case RoomTileMap.AxisMode.XY:
                {
                    Vector3 doorPos = transform.position + (Quaternion.Euler(0,0,transform.eulerAngles.z) * new Vector3(d.position.x,d.position.y,0));
                    Gizmos.DrawCube(doorPos, new Vector3(0.2f,0.2f,0.2f)); 
                    Gizmos.DrawLine(doorPos, doorPos + (Quaternion.Euler(0,0,transform.eulerAngles.z+d.rotation) * new Vector3(0,-1,0))); 
                    Gizmos.DrawLine(doorPos, doorPos + (Quaternion.Euler(0,0,transform.eulerAngles.z+d.rotation) * new Vector3(0,1,0))); 
                    
                    break;
                }
                case RoomTileMap.AxisMode.XZ:
                {    
                    Vector3 doorPos = transform.position + (Quaternion.Euler(0,transform.eulerAngles.y,0) * new Vector3(d.position.x,0,d.position.y));
                    Gizmos.DrawCube(doorPos, new Vector3(0.3f,0.3f,0.3f));  
                    Gizmos.DrawLine(doorPos, doorPos + (Quaternion.Euler(0,transform.eulerAngles.y+d.rotation,0) * new Vector3(0,-1,0))); 
                    Gizmos.DrawLine(doorPos, doorPos + (Quaternion.Euler(0,transform.eulerAngles.y+d.rotation,0) * new Vector3(0,1,0))); 
                    break;
                }
            }  
        } 
    }

    /// <summary>
    /// Calculates the diference in angle of two angles
    /// </summary>
    private float AngleDiff(float angle1, float angle2)
    {
        return Mathf.Abs(DegWrap(DegWrap(angle1) - DegWrap(angle2)));
    }

    /// <summary>
    /// Finds all colliders currently contacting collider1
    /// </summary>
    /// <returns>
    /// a List<(Collider, float)> that stores the colliders that intersect and the distance of a ComputePenetration calculation
    /// </returns>
    private List<(Collider, float)> GetCollisions(Collider collider1, List<PlacedRoomData> rooms)
    {
        List<(Collider, float)> results = new List<(Collider, float)>();
        foreach(PlacedRoomData prd in rooms)
        {
            foreach(Collider collider2 in prd.roomObject.GetComponentInParent<TileData>().colliders3D)
            {
                if(Physics.ComputePenetration(collider1, collider1.gameObject.transform.position, collider1.gameObject.transform.rotation,
                                           collider2, collider2.gameObject.transform.position, collider2.gameObject.transform.rotation, 
                                           out Vector3 direction, out float dist))
                {
                    results.Add((collider2, dist));
                }
            }
        }
        return results;
    }

}
