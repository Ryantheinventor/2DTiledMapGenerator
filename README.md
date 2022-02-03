


# **Random Map Generator**
This system is designed to generate a random map based off of a given tile set. Allowing for required, and rotating, rooms.

NOTE: This system assumes all doors are at the same height.

___
## **Contents**
- [Room Tile Setup](#RoomTileSetup)
- [Tile-Set Setup](#TileSetSetup)
- [Basic Scene Setup](#BasicSceneSetup)
- [API](#API)

___
<a name ="RoomTileSetup"></a>

## **Room Tile Setup:**
### **Step 1:** Create a prefab of a your room
Every object in a room should be contained in a parent object.  
![Image](/ExtraData/PrefabOfRoomSS.png)
  
### **Step 2:** Add Tile Data component
Add the Tile Data to the highest object in the prefab hierarchy.    
![Image](/ExtraData/AddTileDataSS.png)

### **Step 3:** Setup door positions
- Set Gizmo axis mode to the correct mode for your project.
- Add doors to the "Door Positions" list. (When in 3D the Z axis maps to the Y value of doors)
- Set door rotations so that the gizmos point away from the tile as seen below.  
![Image](/ExtraData/AddDoorwaySS.png)

### **Step 4:** Setup colliders
- Either populate the appropriate list with the colliders that you wish to use for overlap detection when generating the map, or if you wish to use all coliders on the object press the detect colliders button.  
![Image](/ExtraData/SetRoomCollidersSS.png)
___



<a name ="TileSetSetup"></a>

## **Tile-Set Setup:**
### **Step 1:** Create new Tile-Set asset
Right click in the project window and go to Create>RandomMapGenerator>RoomTileSet to create a blank tile set...  
![Image](/ExtraData/CreateAssetSS.png)
![Image](/ExtraData/NewAssetSS.png)

### **Step 2:** Open the Tile Set Editor Window
Open the window by double clicking on the asset.  
![Image](/ExtraData/EmptyEditorSS.png)
  
### **Step 3:** Add rooms to tile set
- Add starting room(s) to the "Starting Tile(s)" category.
- Add exit rooms to the "Ending Tile(s)" category.
- Add locked door(s) to the "Door Capping Tile(s)" category.
- Finaly add all other rooms to the "Other Tiles" category.

### **Step 4:** Set "Other Settings"
![Image](/ExtraData/OtherSettingsSS.png)
- Set current mode to the collider type your map uses (They are not cross compatible)[*](#abcd)
- Maximum Collision Overlap determines how much overlap is ignored when rooms are touching when generated.
- Minimum Tiles Per Map is the number of rooms generated before the ending tiles can be spawned.
- Maximum Tiles per Map is the maximum number of tiles the map will generate.

<a name="abcd"></a>*Current Mode also determines how door positions are handled, as long as this setting matches the equivalent on all your tiles then it should work fine.

### **Step 5:** Set room specific settings
![Image](/ExtraData/EditRoomsSS.png)
- Weight determines the likelihood that a room will be chosen when spawning a room, the higher the weight the more often the room will spawn.
- Required will force exactly the set number of that room to be spawned. If required is not set then the room can be used any number of times.
___

<a name ="BasicSceneSetup"></a>

## **Basic Scene Setup:**
### **Step 1:** Create new gameobject at world center
![Image](/ExtraData/NewGOSS.png)  
### **Step 2:** Add the Map Generator component to the new object
![Image](/ExtraData/AddMapGenSS.png)  
### **Step 3:** Set the Tile Set Data to the tile set you wish to use
![Image](/ExtraData/SetTileSetDataSS.png)  
If every thing was done correctly when you press play you should see the generated map once it finishes loading. (By default the generator will generate during the start call, to change this you must use the API)  

___

<a name ="API"></a>

## **API:**

### **Generating a map**
GenerateMap without keeping result data for future use.
```c#
GenerateMap(RoomTileMap tileSet, int maxTries = 100, bool enableDebug = false)
```

GenerateMap and keep data for future use.  
```c#
public bool GenerateMap(RoomTileMap tileSet, out Map mapData, int maxTries = 100, bool enableDebug = false)
```


### **Generating a map with a coroutine.**

#### Coroutine version of GenerateMap:  
```c#
public IEnumerator<MapGenerator.Map> GenerateMapAsync(RoomTileMap tileSet, int maxTries = 100, bool enableDebug = false)
```
While this is a coroutine it can't be run like a normal coroutine as it needs to return custom data.
  
To use this coroutine you will need to setup a MonoBehaviour with the following implementation of the included `CoroutineData` class.  
#### Basic setup of custom coroutine:
```c#
void Start() 
{    
    cd = new CoroutineData(this, mapGenerator.GenerateMapAsync(mapGenerator.tileSetData));
}

void Update() 
{

    if(cd!=null)
    {
        MapGenerator.Map theMap = ((MapGenerator.Map)cd.result);
        if(theMap.isDone)
        {
            Debug.Log("Done Loading Map");
        }
        else
        {
            Debug.Log(theMap.progress);
        }
    }    

}
```