using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RTMG;

namespace RTMGEditor
{
    [CustomEditor(typeof(TileData))]
    public class TileDataInspector : Editor
    {
        private TileData script;

        void OnEnable() 
        {
            script = (TileData)target;
        }

        public override void OnInspectorGUI() 
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            if(GUILayout.Button("Detect colliders"))
            {
                Undo.RecordObject(script, "Detected Colliders");
                script.colliders2D.Clear();
                script.colliders3D.Clear();
                Collider[] collidersFound3D = script.GetComponentsInChildren<Collider>();
                Collider2D[] collidersFound2D = script.GetComponentsInChildren<Collider2D>();
                if(collidersFound2D.Length > collidersFound3D.Length)
                {
                    script.colliders2D = new List<Collider2D>(collidersFound2D);
                    script.gizmoAxisMode = RoomTileMap.AxisMode.IS2D;
                }
                else
                {
                    script.colliders3D = new List<Collider>(collidersFound3D);
                    script.gizmoAxisMode = RoomTileMap.AxisMode.IS3D;
                }
                PrefabUtility.RecordPrefabInstancePropertyModifications(script);
            }


            for(int i = 0; i < script.doorPositions.Count; i++)
            {
                script.doorPositions[i] = new TileData.DoorValues(){ rotation = MapGenerator.DegWrap(script.doorPositions[i].rotation), position = script.doorPositions[i].position};
            } 


            
        }
    }
}