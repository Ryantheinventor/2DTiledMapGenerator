using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using RTMG;

namespace RTMGEditor
{
    class RoomTileMapWindow : EditorWindow 
    {

        

        public RoomTileMap tileMap = null; 
        private RoomTileMap lastMap = null;
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
            WindowSetup(null);
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
                WindowSetup(tileMapAsset);
                return true;
            }
            catch
            {
                return false;
            }
        }

        static void WindowSetup(RoomTileMap tileMap)
        {
            var window = GetWindow<RoomTileMapWindow>();
            window.titleContent = new GUIContent("Room Tile Map Editor");
            Debug.Log(window.minSize + "," + window.maxSize);
            
            window.minSize = new Vector2(225,window.minSize.y);
            window.maxSize = new Vector2(870,window.maxSize.y);
            window.Show();

            if(!(!tileMap && window.tileMap))
            {
                window.tileMap = tileMap;
            }

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
                    int listDisplaySize = DisplayTileList(ref tileMap.startTiles, totalHeight, false);
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
                    listDisplaySize = DisplayTileList(ref tileMap.endTiles, totalHeight, false);
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
                    listDisplaySize = DisplayTileList(ref tileMap.doorCaps, totalHeight, false);
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
                    int displayListHeight = DisplayTileList(ref tileMap.tileSet, totalHeight, true);
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
                    case RoomTileMap.AxisMode.IS2D:
                        modeButtonText += "2D";
                        break;
                    case RoomTileMap.AxisMode.IS3D:
                        modeButtonText += "3D";
                        break;
                }
                if(GUILayout.Button(modeButtonText))
                {
                    switch(tileMap.axisMode)
                    {
                        case RoomTileMap.AxisMode.IS2D:
                            tileMap.axisMode = RoomTileMap.AxisMode.IS3D;
                            break;
                        case RoomTileMap.AxisMode.IS3D:
                            tileMap.axisMode = RoomTileMap.AxisMode.IS2D;
                            break;
                    }
                }
                tileMap.allowRotation = EditorGUILayout.Toggle("Allow Rotation", tileMap.allowRotation);
                tileMap.maxOverlap = EditorGUILayout.FloatField("Maximum Collision Overlap:", tileMap.maxOverlap);
                tileMap.minTileCount = EditorGUILayout.IntField("Minumum Tiles Per Map:", tileMap.minTileCount);
                tileMap.maxTileCount = EditorGUILayout.IntField("Maximum Tiles Per Map:", tileMap.maxTileCount);
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
                SaveAsset(tileMap);
            }
        }

        void OnFocus() 
        {
            if(tileMap)
            {
                ReRenderPrfabPreviews();
            }
        }

        void OnLostFocus() 
        {
            if(tileMap)
            {
                SaveAsset(tileMap);
            }
        }


        void SaveAsset(UnityEngine.Object myAsset)
        {
            // AssetDatabase.SaveAssetIfDirty was added in Unity 2020.3.16
    #if UNITY_2020_3_OR_NEWER && ! (UNITY_2020_3_0 || UNITY_2020_3_1 || UNITY_2020_3_2 || UNITY_2020_3_3 || UNITY_2020_3_4 || UNITY_2020_3_5 || UNITY_2020_3_6 || UNITY_2020_3_7 || UNITY_2020_3_8 || UNITY_2020_3_9 || UNITY_2020_3_10 || UNITY_2020_3_11 || UNITY_2020_3_12 || UNITY_2020_3_13 || UNITY_2020_3_14 || UNITY_2020_3_15)
            AssetDatabase.SaveAssetIfDirty(myAsset);
    #else
            AssetDatabase.SaveAssets();// <- as far as I am aware this is the only fix and I hate that it will save everything.
    #endif
        }

        /// <summary>
        /// returns height of just the displayed list
        /// </summary>
        int DisplayTileList(ref List<RoomTileMap.TileWithState> tileList, int startingHeight, bool canRequire)
        {
            int rowCount = 0;
            List<int> tilesToRemove = new List<int>();

            int prefabDisplayRowCount = Screen.width < 425 ? 1 : Screen.width < 635 ? 2 : Screen.width < 855 ? 3 : 4;

            if(tileList.Count > 0)
            {
                for(int i = 0; i < tileList.Count; i++)
                {
                    if(i % prefabDisplayRowCount == 0)
                    {
                        rowCount++;

                    }
                    float newRarity = tileList[i].rarity;
                    int newRequired = tileList[i].requiredUse;
                    if(PrefabPreview(tileList[i], new Vector2(10 + ((i % prefabDisplayRowCount) * 215),startingHeight + (rowCount-1)*120), out newRarity, out newRequired, canRequire))
                    {
                        tilesToRemove.Add(i);
                    }
                    tileList[i] = new RoomTileMap.TileWithState()
                    {
                        tileObject = tileList[i].tileObject,
                        tileUsable = tileList[i].tileUsable, 
                        previewImage = tileList[i].previewImage,
                        rarity = newRarity,
                        requiredUse = newRequired
                    };
                }
                for(int i = tilesToRemove.Count - 1; i >= 0; i--)
                {
                    tileList.RemoveAt(tilesToRemove[i]);
                }
            }
            return rowCount * 120;
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

            Color[] pixels = tex.GetPixels(0);
            Color testAgainst = pixels[0];
            foreach(Color p in pixels)
            {
                if(p != testAgainst)
                {
                    return tex;
                }
            }

            return null;
        }

        /// <summary>
        /// returns true if the object should be removed
        /// </summary>
        bool PrefabPreview(RoomTileMap.TileWithState tile, Vector2 pos, out float newRarity, out int newRequired, bool canRequire)
        {
            GUI.Box(new Rect(pos.x, pos.y, 200, 110), "");
            GUIStyle s = GUI.skin.GetStyle("Box");
            int length = tile.tileObject.name.Length;
            s.fontSize = length < 13 ? 12 : length < 17 ? 10 : 8;
            s.alignment = TextAnchor.MiddleCenter;
            GUI.Box(new Rect(pos.x, pos.y, 100, 20), tile.tileObject.name, s);
            s.fontSize = 0;
            s.alignment = TextAnchor.UpperCenter;

            if(tile.previewImage)
            {
                GUI.Label(new Rect(pos.x+5, pos.y + 19, 90, 90), tile.previewImage);
            }
            else
            {
                GUIStyle style = GUI.skin.GetStyle("Label");
                style.alignment = TextAnchor.MiddleCenter;
                GUI.Label(new Rect(pos.x+5, pos.y + 19, 90, 90), "Failed to\ngenerate\npreview.", style);
            }


            Color oldGUIColor = GUI.color;
            GUI.color = Color.gray;
            GUI.Box(new Rect(pos.x + 102, pos.y + 24, 96, 40), "");
            GUI.color = oldGUIColor;
            GUI.Box(new Rect(pos.x + 102, pos.y + 24, 96, 40), "Weight: " + tile.rarity);
            
            if(GUI.Button(new Rect(pos.x+153, pos.y + 44, 40, 18), "+"))
                tile.rarity += 1;
            if(GUI.Button(new Rect(pos.x+107, pos.y + 44, 40, 18), "-"))
                tile.rarity -= 1;
            if(tile.rarity < 0)
            {
                tile.rarity = 0;
            }
            newRarity = tile.rarity;

            if(canRequire)
            {
                GUI.color = Color.gray;
                GUI.Box(new Rect(pos.x + 102, pos.y + 67, 96, 40), "");
                GUI.color = oldGUIColor;
                GUI.Box(new Rect(pos.x + 102, pos.y + 67, 96, 40), tile.requiredUse > 0 ? "Required: " + tile.requiredUse : "Not Required");
                
                if(GUI.Button(new Rect(pos.x+153, pos.y + 87, 40, 18), "+"))
                    tile.requiredUse += 1;
                if(GUI.Button(new Rect(pos.x+107, pos.y + 87, 40, 18), "-"))
                    tile.requiredUse -= 1;
                if(tile.requiredUse < 0)
                {
                    tile.requiredUse = 0;
                }
                newRequired = tile.requiredUse;
            }
            else
            {
                newRequired = 0;
            }
            


            return GUI.Button(new Rect(pos.x+101, pos.y + 2, 98, 18), "Remove");
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
                    rarity = tileMap.startTiles[i].rarity,
                    requiredUse = tileMap.startTiles[i].requiredUse
                };
            }
            for(int i = 0; i < tileMap.endTiles.Count; i++)
            {
                tileMap.endTiles[i] = new RoomTileMap.TileWithState()
                {
                    tileObject = tileMap.endTiles[i].tileObject,
                    tileUsable = tileMap.endTiles[i].tileUsable,
                    previewImage = GetPrefabPreview(AssetDatabase.GetAssetPath(tileMap.endTiles[i].tileObject)),
                    rarity = tileMap.endTiles[i].rarity,
                    requiredUse = tileMap.endTiles[i].requiredUse
                };
            }
            for(int i = 0; i < tileMap.tileSet.Count; i++)
            {
                tileMap.tileSet[i] = new RoomTileMap.TileWithState()
                {
                    tileObject = tileMap.tileSet[i].tileObject,
                    tileUsable = tileMap.tileSet[i].tileUsable,
                    previewImage = GetPrefabPreview(AssetDatabase.GetAssetPath(tileMap.tileSet[i].tileObject)),
                    rarity = tileMap.tileSet[i].rarity,
                    requiredUse = tileMap.tileSet[i].requiredUse
                };
            }
            for(int i = 0; i < tileMap.doorCaps.Count; i++)
            {
                tileMap.doorCaps[i] = new RoomTileMap.TileWithState()
                {
                    tileObject = tileMap.doorCaps[i].tileObject,
                    tileUsable = tileMap.doorCaps[i].tileUsable,
                    previewImage = GetPrefabPreview(AssetDatabase.GetAssetPath(tileMap.doorCaps[i].tileObject)),
                    rarity = tileMap.doorCaps[i].rarity,
                    requiredUse = tileMap.doorCaps[i].requiredUse
                };
            }
        }
    }
}