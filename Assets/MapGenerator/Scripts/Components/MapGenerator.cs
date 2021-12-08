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

        //place starting tile
        TileWithState startingTileData = tileSet.startTiles[Random.Range(0,tileSet.startTiles.Count)];
        float rotation = tileSet.allowRotation ? 90 * Random.Range(0,4) : 0;
        
        PlaceRoom(startingTileData.tileObject, new Vector2(0,0), rotation, tileSet.axisMode, -1, out List<DoorLocation> newRooms, out PlacedRoomData newRoomData, 0);
        placedRooms.Add(newRoomData);
        openDoors.AddRange(newRooms);
        int tempStop = 10;
        while(openDoors.Count > 0)
        {
            tempStop -= 1;
            if(GenerateRoom(0, openDoors, unlockedDoors, lockedDoors, placedRooms, tileSet))
            {
                unlockedDoors.Add(openDoors[0]);
                openDoors.RemoveAt(0);
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

        return false;
    }

    private bool GenerateRoom(int doorIndex, 
                              List<DoorLocation> openDoors, 
                              List<DoorLocation> unlockedDoors,
                              List<DoorLocation> lockedDoors, 
                              List<PlacedRoomData> placedRooms, 
                              RoomTileMap tileSet)
    {
        Vector2 targetDoorPos = openDoors[doorIndex].position;
        float[] roomProbabilities = new float[tileSet.tileSet.Count];
        for(int i = 0; i < roomProbabilities.Length; i++)
        {
            roomProbabilities[i] = 1;
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
            roomFits = CheckRoomFits(openDoors[doorIndex], tileSet.tileSet[roomIndex], placedRooms, out Vector2 position, 
                          out float rotation, out PlacedRoomData newRoom, out List<DoorLocation> newDoors, 
                          tileSet, openDoors[doorIndex].roomIndex);
            placedRooms.Add(newRoom);
            openDoors.AddRange(newDoors);

        }
        return true;


    }

    private GameObject PlaceRoom(GameObject room, Vector2 position, float rotation, AxisMode axisMode, 
                                 int ignoreDoorIndex, out List<DoorLocation> newOpenDoors, out PlacedRoomData placedRoom, int roomIndex)
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
        
        TileData td = tileObject.GetComponent<TileData>();
        td.gizmoAxisMode = axisMode;

        //add new doors to the openDoors list
        newOpenDoors = new List<DoorLocation>();
        for(int i = 0; i < td.doorPositions.Count; i++)
        {
            if(i != ignoreDoorIndex)
            {
                Vector3 rotDoorPosV3 = Quaternion.Euler(0,0,rotation) * new Vector3(td.doorPositions[i].position.x, td.doorPositions[i].position.y, 0);
                Vector2 mapSpaceDoorPos = new Vector2(rotDoorPosV3.x,rotDoorPosV3.y) + position;
                newOpenDoors.Add(new DoorLocation(){ position = mapSpaceDoorPos, rotation = DegWrap(td.doorPositions[i].rotation + rotation), roomIndex = roomIndex });
            }
        }

        placedRoom = new PlacedRoomData(){ position = position, rotation = rotation, scale = td.tileSize };



        return tileObject;
    }

    private bool CheckRoomFits(DoorLocation targetDoor, TileWithState tile, List<PlacedRoomData> placedRooms, out Vector2 position, 
                               out float rotation, out PlacedRoomData newRoom, out List<DoorLocation> newDoors, RoomTileMap tileSet, int roomIndex)
    {
        newRoom = new PlacedRoomData();
        newDoors = new List<DoorLocation>();

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

            //find the rotation by matching the rotation of the cur door to the 180 degree ooposite of target door
            float targetRot = DegWrap(targetDoor.rotation + 180);
            rotation = targetRot - DegWrap(doorPosition.rotation);

            //move the position to the correct place
            Vector3 rotDoorPosV3 = Quaternion.Euler(0,0,rotation) * new Vector3(doorPosition.position.x, doorPosition.position.y, 0);
            position = targetDoor.position - new Vector2(rotDoorPosV3.x,rotDoorPosV3.y); 

            //do collision check here
            GameObject newRoomObject = PlaceRoom(tile.tileObject, position, rotation, tileSet.axisMode, doorIndex, out List<DoorLocation> newOpenDoors, out PlacedRoomData newRoomData, roomIndex);
            newDoors.AddRange(newOpenDoors);
            newRoom = newRoomData;
            if(newRoomObject.GetComponent<TileData>().colliders2D.Count > 0)
            {
                //do 2d collision checks
            }
            else 
            {
                //do 3d collision checks
            }
            return true;

        }

        

        //Vector3 rotatedDoorPos = Quaternion.Euler(0,0,rotation) * new Vector3(doorPosition.x,doorPosition.y,0);

        //return false
        position = new Vector2(0,0);
        rotation = 0;
        return false;
    }


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
        float curVal = probabilities[0];
        while(target > curVal)
        {
            curVal += probabilities[index];
            index++;
        }
        //for just incase floating points cause a weird value
        if(index >= probabilities.Length)
        {
            index = probabilities.Length - 1;
        }
        return index;
    }

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
            Gizmos.color = Color.red;
            switch(tileSetData.axisMode)
            {
                case RoomTileMap.AxisMode.XY:
                {
                    Vector3 doorPos = transform.position + (Quaternion.Euler(0,0,transform.eulerAngles.z) * new Vector3(d.position.x,d.position.y,0));
                    Gizmos.DrawCube(doorPos, new Vector3(0.2f,0.2f,0.2f)); 
                    Gizmos.DrawLine(doorPos, doorPos + (Quaternion.Euler(0,0,transform.eulerAngles.z+d.rotation) * new Vector3(1,0,0))); 
                    break;
                }
                case RoomTileMap.AxisMode.XZ:
                {    
                    Vector3 doorPos = transform.position + (Quaternion.Euler(0,transform.eulerAngles.y,0) * new Vector3(d.position.x,0,d.position.y));
                    Gizmos.DrawCube(doorPos, new Vector3(0.2f,0.2f,0.2f));  
                    Gizmos.DrawLine(doorPos, doorPos + (Quaternion.Euler(0,transform.eulerAngles.y+d.rotation,0) * new Vector3(1,0,0))); 
                    break;
                }
            }  
        } 

        foreach(DoorLocation d in mgOpenDoors)
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
}
