using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;

class RoomTileMapWindow : EditorWindow 
{

    private int prefabDisplayRowCount = 5;

    public RoomTileMap tileMap = null; 
    private RoomTileMap lastMap = null;
    private GameObject testObject = null;
    private bool specialRoomsFoldout = true;
    private bool defaultFoldout = true;
    private int totalHeight = 0;

    private Vector2 scrollPos = new Vector2(0,0);

    /// <summary>
    /// Opens the window in the editor
    /// </summary>
    [MenuItem("Window/RandomMapGenerator/RoomTileMapEditor")]
    static void ShowWindow() 
    {
        var window = GetWindow<RoomTileMapWindow>();
        window.titleContent = new GUIContent("RoomTileMapEditor");
        window.Show();
    }

    /// <summary>
    /// Called by Unity to update the window
    /// </summary>
    void OnGUI() 
    {
        
        totalHeight = 0;
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos,true,false);
        GUILayout.Label("Tile Map", EditorStyles.boldLabel);
        tileMap = (RoomTileMap)EditorGUILayout.ObjectField((UnityEngine.Object)tileMap,typeof(RoomTileMap),true);
        //only re render if the current tilemap is a different one than the last
        if(lastMap != tileMap)
        {
            lastMap = tileMap;
            ReRenderPrfabPreviews();
        }
        GuiLine();
        totalHeight += 41;
        //display the rest of the editor
        if(tileMap)
        {
            #region special tile lists
            //display special rooms like start and ending rooms
            specialRoomsFoldout = EditorGUILayout.Foldout(specialRoomsFoldout, "Special Tile Types");
            totalHeight += 20;
            if(specialRoomsFoldout)
            {   
                #region StartingTile
                
                GUILayout.Label("Starting Tile(s):", EditorStyles.boldLabel);
                GameObject newStartObject = (GameObject)EditorGUILayout.ObjectField("Add new starting tile:", null, typeof(GameObject), false);
                totalHeight += 40;

                if(newStartObject)
                {
                    tileMap.startTiles.Add(new RoomTileMap.TileWithState()
                    {
                        tileObject = newStartObject,
                        tileUsable = true, 
                        previewImage = GetPrefabPreview(AssetDatabase.GetAssetPath(newStartObject)) ,
                        rarity = 1f
                    });
                }

                //draw every starting tile onto the window
                int listDisplaySize = DisplayTileList(ref tileMap.startTiles, totalHeight);
                EditorGUILayout.Space(listDisplaySize);
                totalHeight += listDisplaySize;
                
                #endregion
                
                GuiLine();
                totalHeight += 1;

                #region EndingTile

                GUILayout.Label("Ending Tile(s):", EditorStyles.boldLabel);
                GameObject newEndObject = (GameObject)EditorGUILayout.ObjectField("Add new ending tile:", null, typeof(GameObject), false);
                totalHeight += 40;//acount for Text Label for the Exit Room catagories

                if(newEndObject)
                {
                    tileMap.endTiles.Add(new RoomTileMap.TileWithState()
                    {
                        tileObject = newEndObject,
                        tileUsable = true, 
                        previewImage = GetPrefabPreview(AssetDatabase.GetAssetPath(newEndObject)) ,
                        rarity = 1f
                    });
                }

                //draw every ending tile onto the window
                listDisplaySize = DisplayTileList(ref tileMap.endTiles, totalHeight);
                EditorGUILayout.Space(listDisplaySize);
                totalHeight += listDisplaySize;
                
                #endregion

                GuiLine();
                totalHeight += 1;

                #region EndingTile

                GUILayout.Label("Door Caping Tile(s):", EditorStyles.boldLabel);
                GameObject newCap = (GameObject)EditorGUILayout.ObjectField("Add new door cap tile:", null, typeof(GameObject), false);
                totalHeight += 40;//acount for Text Label for the Exit Room catagories

                if(newCap)
                {
                    tileMap.doorCaps.Add(new RoomTileMap.TileWithState()
                    {
                        tileObject = newCap,
                        tileUsable = true, 
                        previewImage = GetPrefabPreview(AssetDatabase.GetAssetPath(newCap)) ,
                        rarity = 1f
                    });
                }

                //draw every ending tile onto the window
                listDisplaySize = DisplayTileList(ref tileMap.doorCaps, totalHeight);
                EditorGUILayout.Space(listDisplaySize);
                totalHeight += listDisplaySize;
                
                #endregion
            }
            GuiLine();
            totalHeight += 1;
            #endregion

            #region normal tile list
            defaultFoldout = EditorGUILayout.Foldout(defaultFoldout, "Other Tiles");
            totalHeight += 24;
            if(defaultFoldout)
            {   
                GameObject newStandardObject = (GameObject)EditorGUILayout.ObjectField("Add new tile:", null, typeof(GameObject), false);
                totalHeight += 20;
                if(newStandardObject)
                {
                    tileMap.tileSet.Add(new RoomTileMap.TileWithState()
                    {
                        tileObject = newStandardObject,
                        tileUsable = true, 
                        previewImage = GetPrefabPreview(AssetDatabase.GetAssetPath(newStandardObject)),
                        rarity = 1f
                    });
                }

                //draw every tile onto the window
                int displayListHeight = DisplayTileList(ref tileMap.tileSet, totalHeight);
                EditorGUILayout.Space(displayListHeight);
                totalHeight += displayListHeight;
            }
            GuiLine();
            totalHeight += 1;
            #endregion

            #region other settings
            GUILayout.Label("Other Settings:", EditorStyles.boldLabel);
            string modeButtonText = "Current Mode: ";
            switch(tileMap.axisMode)
            {
                case RoomTileMap.AxisMode.XY:
                    modeButtonText += "X/Y (map spreads across the X and Y axis, good for 2D games)";
                    break;
                case RoomTileMap.AxisMode.XZ:
                    modeButtonText += "X/Z (map spreads across the X and Z axis, good for 3D games)";
                    break;
            }
            if(GUILayout.Button(modeButtonText))
            {
                switch(tileMap.axisMode)
                {
                    case RoomTileMap.AxisMode.XY:
                        tileMap.axisMode = RoomTileMap.AxisMode.XZ;
                        break;
                    case RoomTileMap.AxisMode.XZ:
                        tileMap.axisMode = RoomTileMap.AxisMode.XY;
                        break;
                }
            }
            tileMap.allowRotation = EditorGUILayout.Toggle("Allow Rotation", tileMap.allowRotation);
            tileMap.maxOverlap = EditorGUILayout.FloatField("Maximum Collision Overlap:", tileMap.maxOverlap);
            tileMap.maxTileCount = EditorGUILayout.IntField("Maximum Tiles Per Map:", tileMap.maxTileCount);
            tileMap.minTileCount = EditorGUILayout.IntField("Minumum Tiles Per Map:", tileMap.minTileCount);
            totalHeight += 40;
            #endregion
            EditorUtility.SetDirty(tileMap);
            
        }
        EditorGUILayout.EndScrollView();
    }

    void OnDestroy() 
    {
        if(tileMap)
        {
            AssetDatabase.SaveAssetIfDirty(tileMap);
            Debug.Log("Saving asset");
        }
    }

    void OnLostFocus() 
    {
        if(tileMap)
        {
            AssetDatabase.SaveAssetIfDirty(tileMap);
            Debug.Log("Saving asset");
        }
    }

    /// <summary>
    /// returns height of just the displayed list
    /// </summary>
    int DisplayTileList(ref List<RoomTileMap.TileWithState> tileList, int startingHeight)
    {
        int rowCount = 0;
        List<int> tilesToRemove = new List<int>();
        if(tileList.Count > 0)
        {
            for(int i = 0; i < tileList.Count; i++)
            {
                if(i % prefabDisplayRowCount == 0)
                {
                    rowCount++;
                }
                float newRarity = tileList[i].rarity;
                if(PrefabPreview(tileList[i], new Vector2(10 + ((i % prefabDisplayRowCount) * 120),startingHeight + (rowCount-1)*160), out newRarity))
                {
                    tilesToRemove.Add(i);
                }
                tileList[i] = new RoomTileMap.TileWithState()
                {
                    tileObject = tileList[i].tileObject,
                    tileUsable = tileList[i].tileUsable, 
                    previewImage = tileList[i].previewImage,
                    rarity = newRarity  
                };
            }
            for(int i = tilesToRemove.Count - 1; i >= 0; i--)
            {
                tileList.RemoveAt(tilesToRemove[i]);
            }
        }
        return rowCount * 161;
    }

    /// <summary>
    /// Draws a line across the window
    /// </summary>
    void GuiLine(int i_height = 1)
    {
        GuiLine(new Color(0.5f, 0.5f, 0.5f, 1f), i_height);
    }

    /// <summary>
    /// Draws a colored line across the window
    /// </summary>
    void GuiLine(Color drawColor, int i_height = 1)
    {
        Rect rect = EditorGUILayout.GetControlRect(false, i_height );

        rect.height = i_height;

        EditorGUI.DrawRect(rect, drawColor);
    }

    /// <summary>
    /// generates a texture2D for the path given
    /// </summary>
    static Texture2D GetPrefabPreview(string path)
    {
        //Debug.Log("Generating Preview for:" + path);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        var editor = UnityEditor.Editor.CreateEditor(prefab);
        Texture2D tex = editor.RenderStaticPreview(path, null, 200, 200);
        EditorWindow.DestroyImmediate(editor);
        return tex;
    }

    /// <summary>
    /// returns true if the object should be removed
    /// </summary>
    bool PrefabPreview(RoomTileMap.TileWithState tile, Vector2 pos, out float newRarity)
    {
        GUI.Box(new Rect(pos.x, pos.y, 100, 160), "");
        GUI.Box(new Rect(pos.x, pos.y, 100, 20), tile.tileObject.name);

        GUI.Label(new Rect(pos.x+10, pos.y + 21, 80, 80), tile.previewImage);

        Color oldGUIColor = GUI.color;
        GUI.color = Color.gray;
        GUI.Box(new Rect(pos.x + 2, pos.y + 100, 96, 40), "");
        GUI.color = oldGUIColor;
        GUI.Box(new Rect(pos.x + 2, pos.y + 100, 96, 40), "Weight:" + tile.rarity);
        
        if(GUI.Button(new Rect(pos.x+7, pos.y + 120, 40, 18), "+"))
            tile.rarity += 1;
        if(GUI.Button(new Rect(pos.x+53, pos.y + 120, 40, 18), "-"))
            tile.rarity -= 1;
            if(tile.rarity < 0)
            {
                tile.rarity = 0;
            }
        newRarity = tile.rarity;
        return GUI.Button(new Rect(pos.x+1, pos.y + 140, 98, 18), "Remove");
    }

    /// <summary>
    /// re-generates the preview images and applies them to the tilemap asset
    /// </summary>
    void ReRenderPrfabPreviews()
    {
        //Debug.Log("Re-Render");
        for(int i = 0; i < tileMap.startTiles.Count; i++)
        {
            tileMap.startTiles[i] = new RoomTileMap.TileWithState()
            {
                tileObject = tileMap.startTiles[i].tileObject,
                tileUsable = tileMap.startTiles[i].tileUsable,
                previewImage = GetPrefabPreview(AssetDatabase.GetAssetPath(tileMap.startTiles[i].tileObject)),
                rarity = tileMap.startTiles[i].rarity
            };
        }
        for(int i = 0; i < tileMap.endTiles.Count; i++)
        {
            tileMap.endTiles[i] = new RoomTileMap.TileWithState()
            {
                tileObject = tileMap.endTiles[i].tileObject,
                tileUsable = tileMap.endTiles[i].tileUsable,
                previewImage = GetPrefabPreview(AssetDatabase.GetAssetPath(tileMap.endTiles[i].tileObject)),
                rarity = tileMap.endTiles[i].rarity
            };
        }
        for(int i = 0; i < tileMap.tileSet.Count; i++)
        {
            tileMap.tileSet[i] = new RoomTileMap.TileWithState()
            {
                tileObject = tileMap.tileSet[i].tileObject,
                tileUsable = tileMap.tileSet[i].tileUsable,
                previewImage = GetPrefabPreview(AssetDatabase.GetAssetPath(tileMap.tileSet[i].tileObject)),
                rarity = tileMap.tileSet[i].rarity
            };
        }
        for(int i = 0; i < tileMap.doorCaps.Count; i++)
        {
            tileMap.doorCaps[i] = new RoomTileMap.TileWithState()
            {
                tileObject = tileMap.doorCaps[i].tileObject,
                tileUsable = tileMap.doorCaps[i].tileUsable,
                previewImage = GetPrefabPreview(AssetDatabase.GetAssetPath(tileMap.doorCaps[i].tileObject)),
                rarity = tileMap.doorCaps[i].rarity
            };
        }
    }

    /// <summary>
    /// handles opening the settings asset in the unity editor
    /// </summary>
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
