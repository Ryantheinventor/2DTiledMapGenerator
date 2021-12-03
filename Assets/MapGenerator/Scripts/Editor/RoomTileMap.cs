using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;

[CreateAssetMenu(fileName = "RoomTileMap", menuName = "RandomMapGenerator/RoomTileMap", order = 0)]
public class RoomTileMap : ScriptableObject 
{
    
    public List<TileWithState> startTiles = new List<TileWithState>();
    public List<TileWithState> endTiles = new List<TileWithState>();

    public List<TileWithState> tileSet = new List<TileWithState>();

    public AxisMode axisMode = AxisMode.XY;

    [Serializable]
    public struct TileWithState
    {
        public GameObject tileObject;
        public bool tileUsable;
        public Texture2D previewImage;
    }

    [Serializable]
    public enum AxisMode
    {
        XY,//2D
        XZ//3D
    }


    [OnOpenAssetAttribute(1)]
    public static bool OpenAsset(int instanceID, int line)
    {
        try
        {
            RoomTileMap tileMapAsset = (RoomTileMap)EditorUtility.InstanceIDToObject(instanceID);
            RoomTileMapWindow window = (RoomTileMapWindow) EditorWindow.GetWindow( typeof(RoomTileMapWindow), false, "RoomTileMapEditor" );
            window.tileMap = tileMapAsset;
            window.Show();
            return true;
        }
        catch(InvalidCastException e)
        {
            return false;
        }
        
        
    }
}
