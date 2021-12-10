using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTracker : MonoBehaviour
{
    public GameObject trackedObject;
    
    public int doorID = 0;

    private TileData td;


    // Start is called before the first frame update
    void Start()
    {
        td = GetComponent<TileData>();
    }

    // Update is called once per frame
    void Update()
    {
        trackedObject.transform.eulerAngles += new Vector3(0,90,0) * Time.deltaTime;


        //find the rotation by matching the rotation of the cur door to the 180 degree ooposite of target door
        float targetRot = DegWrap(trackedObject.transform.eulerAngles.y + 180);
        float rotation = targetRot - DegWrap(td.doorPositions[doorID].rotation);


        Vector3 rotDoorPosV3 = Quaternion.Euler(0,rotation,0) * new Vector3(td.doorPositions[doorID].position.x, 0, td.doorPositions[doorID].position.y);
        Vector2 position = new Vector2(trackedObject.transform.position.x, trackedObject.transform.position.z) - new Vector2(rotDoorPosV3.x,rotDoorPosV3.z); 






        transform.position = new Vector3(position.x,0,position.y);
        transform.eulerAngles = new Vector3(0,rotation,0);

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
}
