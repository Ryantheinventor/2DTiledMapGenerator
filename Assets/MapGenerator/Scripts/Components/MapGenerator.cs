using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static RoomTileMap;
public class MapGenerator : MonoBehaviour
{
    public RoomTileMap tileSetData;
    public bool generateOnStart = true;

    public List<DoorLocation> Doorways {get => mgDoorways;}
    public List<PlacedRoomData> PlacedRooms {get => mgPlacedRooms;}

    private List<DoorLocation> mgDoorways = new List<DoorLocation>();
    private List<PlacedRoomData> mgPlacedRooms = new List<PlacedRoomData>();

    public struct DoorLocation
    {
        public Vector2 position;
        public float rotation;

        public int roomIndex;
        public bool isLocked;
    }

    public struct PlacedRoomData
    {
        public GameObject roomObject;
        public Vector2 position;
        public Vector2 scale;
        public float rotation;
    }

    public class Map
    {
        public bool isDone = false;
        public bool failed = false;
        public float progress = 0;
        public List<PlacedRoomData> rooms;
        public List<DoorLocation> doorways;

        public Map()
        {
            rooms = new List<PlacedRoomData>();
            doorways = new List<DoorLocation>();
        }

    }


    void Start() 
    {
        if(generateOnStart)
        {
            GenerateMap(tileSetData, out mgDoorways, out mgPlacedRooms, 1000, true);
        }
    }


    /// <summary>
    /// Attempts to generate a map based off of the provided tile set
    /// </summary>
    ///<returns>
    /// Returns true when the map was fully generated, returns false otherwise.
    ///</returns>
    public bool GenerateMap(RoomTileMap tileSet, int maxTries = 100, bool enableDebug = false)
    {   
        return GenerateMap(tileSet, out List<DoorLocation> doorways, out List<PlacedRoomData> placedRooms);        
    }


    /// <summary>
    /// Attempts to generate a map based off of the provided tile set
    /// </summary>
    ///<returns>
    /// Returns true when the map was fully generated, returns false otherwise.
    ///</returns>
    public bool GenerateMap(RoomTileMap tileSet, out List<DoorLocation> doorways, out List<PlacedRoomData> placedRooms, int maxTries = 100, bool enableDebug = false)
    {   
        IEnumerator target = GenerateMapAsync(tileSet, maxTries, enableDebug);
        
        while(target.MoveNext())
        {
            if(((Map)target.Current).isDone)
            {
                break;
            }
        }
        Map mapData = ((Map)target.Current);
        doorways = mapData.doorways;
        placedRooms = mapData.rooms;
        return !((Map)target.Current).failed;
        //return GenerateMap(tileSet, out doorways, out placedRooms, maxTries, enableDebug);
    }

    /// <summary>
    /// Attempts to generate a map based off of the provided tile set
    /// </summary>
    ///<returns>
    /// Returns a Map object containing info about the generated map
    ///</returns>
    public IEnumerator<Map> GenerateMapAsync(RoomTileMap tileSet, int maxTries = 100, bool enableDebug = false)
    {
        Map myMap = new Map();
        List<DoorLocation> openDoors = new List<DoorLocation>(); //list of all doors that have not had an attempted tile placed at the door
        List<DoorLocation> blockedDoors = new List<DoorLocation>(); //list of doors that were blocked when generating
        int[] roomUsedCounts = new int[tileSet.tileSet.Count];
        
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
            myMap.isDone = true;
            myMap.failed = true;
            yield return myMap;
            yield break;
        }
        #endregion
        
        int maxAttempts = maxTries;
        bool endingMade = false;
        while(((!endingMade || !RequiredRoomsSpawned(roomUsedCounts, tileSet)) || myMap.rooms.Count < tileSet.minTileCount) && maxAttempts > 0)
        {
            maxAttempts--;

            //reset data for just incase this is not the first time around
            foreach(PlacedRoomData prd in myMap.rooms)
            {
                Destroy(prd.roomObject);
            }
            endingMade = false;
            openDoors.Clear();
            myMap.rooms.Clear();
            blockedDoors.Clear();
            myMap.doorways.Clear();
            roomUsedCounts = new int[tileSet.tileSet.Count];


            //place starting tile
            TileWithState startingTileData = tileSet.startTiles[Random.Range(0,tileSet.startTiles.Count)];
            float rotation = tileSet.allowRotation ? 90 * Random.Range(0,4) : 0;
            
            PlaceRoom(startingTileData.tileObject, new Vector2(0,0), rotation, tileSet.axisMode, -1, out List<DoorLocation> newRooms, out PlacedRoomData newRoomData, 0, tileSet.useRB2D);
            myMap.rooms.Add(newRoomData);
            openDoors.AddRange(newRooms);
            
            //the maximum amount of tiles placed
            int tempStop = tileSet.maxTileCount;
            int roomsPlaced = 0;
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            while(openDoors.Count > 0)
            {
                if(!sw.IsRunning)
                {
                    sw.Start();
                }
                if(GenerateRoom(0, openDoors, myMap.rooms, tileSet, tileSet.minTileCount < myMap.rooms.Count && !endingMade, out bool usedExit, ref roomUsedCounts, roomsPlaced))
                {
                    if(usedExit)
                    {
                        endingMade = true;
                    }
                    myMap.doorways.Add(openDoors[0]);
                    openDoors.RemoveAt(0);
                    tempStop -= 1;
                    roomsPlaced++;
                }
                else
                {
                    blockedDoors.Add(openDoors[0]);
                    openDoors.RemoveAt(0);
                }
                if(tempStop <= 0)
                {
                    break;
                }
                myMap.progress = (float)roomsPlaced/(tileSet.maxTileCount)*0.9f;//add 5 to have head room for future parts.
                if(sw.ElapsedMilliseconds >= 30)
                {
                    sw.Reset();
                    yield return myMap;
                }
            }
        }
        bool validMap = true;
        //if we still dont have enough rooms we give up
        if(myMap.rooms.Count < tileSet.minTileCount || (!endingMade || !RequiredRoomsSpawned(roomUsedCounts, tileSet)))
        {
            Debug.LogError("Something prevented a map from being fully generated. This can be caused by not enough tile varients, poorly weighted tiles, or a pre existing object is blocking collision.");
            validMap = false;
        }

        openDoors.AddRange(blockedDoors);
        blockedDoors.Clear();
        float cOpenDoors = openDoors.Count;
        //combine doors that are in the same place
        System.Diagnostics.Stopwatch stop2 = new System.Diagnostics.Stopwatch();
        while(openDoors.Count > 1)
        {
            if(!stop2.IsRunning)
            {
                stop2.Start();
            }
            myMap.progress = 0.9f+(1-(openDoors.Count/cOpenDoors))*0.09f;
            if(stop2.ElapsedMilliseconds >= 40)
            {
                stop2.Reset();
                yield return myMap;
            }
            bool foundOne = false;
            for(int i = 1; i < openDoors.Count; i++)
            {
                if(Vector2.Distance(openDoors[0].position, openDoors[i].position) < 0.1f)
                {
                    if(AngleDiff(openDoors[0].rotation, openDoors[i].rotation) > 0.1f)
                    {
                        myMap.doorways.Add(openDoors[0]);
                        openDoors.RemoveAt(i);
                        openDoors.RemoveAt(0);
                        foundOne = true;
                        break;
                    }
                }
            }
            if(!foundOne)
            {
                DoorLocation d = openDoors[0];
                d.isLocked = true;
                myMap.doorways.Add(d);
                openDoors.RemoveAt(0);
            }
        }


        //add any remaining doors to the locked door catagory
        for(int i = 0; i < openDoors.Count; i++)
        {
            DoorLocation d = openDoors[i];
            d.isLocked = true;
            myMap.doorways.Add(d);
        }
        openDoors.Clear();

        //add locked door objects to doorways that do not lead to anything
        float[] doorProbabilities = new float[tileSet.doorCaps.Count];
        for(int i = 0; i < doorProbabilities.Length; i++)
        {
            doorProbabilities[i] = tileSet.doorCaps[i].rarity;
        }
        foreach(DoorLocation dl in myMap.doorways)
        {   
            if(dl.isLocked)
            {
                int doorIndex = SkewedNum(doorProbabilities);
                GameObject newDoorFab = tileSet.doorCaps[doorIndex].tileObject;
                PlaceRoom(newDoorFab, dl.position, dl.rotation, tileSet.axisMode, 0, out List<DoorLocation> newOpenDoors, out PlacedRoomData newDoor, 0, tileSet.useRB2D);
            }
            
        }
        myMap.progress = 1f;
        myMap.isDone = true;
        myMap.failed = !validMap;
        yield return myMap;
        yield break;
    }


    /// <summary>
    /// picks a room at random untill a room fits
    /// </summary>
    /// <returns>
    /// true a room was placed
    /// </returns>
    private bool GenerateRoom(int doorIndex, 
                              List<DoorLocation> openDoors, 
                              List<PlacedRoomData> placedRooms, 
                              RoomTileMap tileSet, 
                              bool allowEndingRooms, out bool usedEndingRoom,
                              ref int[] roomUsedCounts, int roomsPlaced)
    {
        usedEndingRoom = false;
        Vector2 targetDoorPos = openDoors[doorIndex].position;
        //add endTiles to probability if allowEndingRooms is true
        float[] roomProbabilities = new float[allowEndingRooms ? tileSet.tileSet.Count + tileSet.endTiles.Count : tileSet.tileSet.Count];
        for(int i = 0; i < roomProbabilities.Length; i++)
        {
            if(i >= tileSet.tileSet.Count)
            {
                roomProbabilities[i] = tileSet.endTiles[i-tileSet.tileSet.Count].rarity * 2;
            }
            else
            {
                if(tileSet.tileSet[i].requiredUse > 0)
                {
                    if(roomUsedCounts[i] < tileSet.tileSet[i].requiredUse)
                    {
                        if(roomsPlaced > tileSet.minTileCount)
                        {
                            roomProbabilities[i] = (1 + tileSet.tileSet[i].rarity) * roomsPlaced * (tileSet.tileSet[i].rarity / tileSet.maxTileCount);
                        }
                        else
                        {
                            roomProbabilities[i] = tileSet.tileSet[i].rarity;
                        }
                    }
                    else
                    {
                        roomProbabilities[i] = 0;
                    }
                    
                }
                else
                {
                    roomProbabilities[i] = tileSet.tileSet[i].rarity;
                }
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
                if(roomIndex < tileSet.tileSet.Count)
                {
                    roomUsedCounts[roomIndex]++;
                }
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
                    List<(Collider2D, float)> results = GetCollisions(c, placedRooms);
                    if(results.Count > 0)
                    {
                        foreach((Collider2D, float) c2 in results)
                        {
                            if(!td.colliders2D.Contains(c2.Item1))
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
            case AxisMode.IS2D:
                tileObject.transform.position = new Vector3(position.x, position.y, 0);
                tileObject.transform.eulerAngles = new Vector3(0, 0, rotation);
                break;
            case AxisMode.IS3D:
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
    public static float DegWrap(float original)
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
        foreach(DoorLocation d in mgDoorways)
        {
            if(d.isLocked)
            {
                Gizmos.color = Color.magenta;
                switch(tileSetData.axisMode)
                {
                    case RoomTileMap.AxisMode.IS2D:
                    {
                        Vector3 doorPos = transform.position + (Quaternion.Euler(0,0,transform.eulerAngles.z) * new Vector3(d.position.x,d.position.y,0));
                        Gizmos.DrawCube(doorPos, new Vector3(0.2f,0.2f,0.2f)); 
                        Gizmos.DrawLine(doorPos, doorPos + (Quaternion.Euler(0,0,transform.eulerAngles.z+d.rotation) * new Vector3(0,-1,0))); 
                        Gizmos.DrawLine(doorPos, doorPos + (Quaternion.Euler(0,0,transform.eulerAngles.z+d.rotation) * new Vector3(0,1,0))); 
                        
                        break;
                    }
                    case RoomTileMap.AxisMode.IS3D:
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
    }

    /// <summary>
    /// Calculates the diference in angle of two angles
    /// </summary>
    public static float AngleDiff(float angle1, float angle2)
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

    private List<(Collider2D, float)> GetCollisions(Collider2D collider1, List<PlacedRoomData> rooms)
    {
        List<(Collider2D, float)> results = new List<(Collider2D, float)>();
        foreach(PlacedRoomData prd in rooms)
        {
            foreach(Collider2D collider2 in prd.roomObject.GetComponentInParent<TileData>().colliders2D)
            {
                ColliderDistance2D cd = Physics2D.Distance(collider1,collider2);
                if(cd.isOverlapped)
                {
                    results.Add((collider2, -cd.distance));
                }
            }
        }
        return results;
    }

    private bool RequiredRoomsSpawned(int[] roomSpawnCounts, RoomTileMap tileSet)
    {
        
        for(int i = 0; i < tileSet.tileSet.Count; i++)
        {
            TileWithState t = tileSet.tileSet[i];
            if(t.requiredUse > 0 && !(t.requiredUse == roomSpawnCounts[i]))
            {
                return false;
            }
        }
        return true;
    }


}
