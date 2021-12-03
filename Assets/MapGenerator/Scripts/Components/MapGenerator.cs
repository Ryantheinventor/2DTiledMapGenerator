using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static RoomTileMap;
public class MapGenerator : MonoBehaviour
{
    public RoomTileMap tileSetData;
    public bool generateOnStart = true;

    void Start() 
    {
        if(generateOnStart)
        {
            GenerateMap(tileSetData, true);
        }
    }


    /// <summary>
    /// Attempts to generate a map based off of the provided tile set
    /// </summary>
    ///<returns>
    /// Returns true when the map was fully generated, returns false otherwise.
    ///</returns>
    public bool GenerateMap(RoomTileMap tileSet, bool enableDebug = false)
    {
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
        PlaceRoom(startingTileData.tileObject, new Vector2(0,0), rotation, tileSet.axisMode);

        return false;
    }


    private GameObject PlaceRoom(GameObject room, Vector2 position, float rotation, AxisMode axisMode)
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
        return tileObject;
    }

}
