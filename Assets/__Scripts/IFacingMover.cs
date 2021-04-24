using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFacingMover
{
    int GetFacing();
    bool Moving 
    { 
        get; 
    }
    float GetSpeed();
    float GridMult
    {
        get;
    }
    Vector2 RoomPos
    {
        get;
        set;
    }
    Vector2 RoomNum
    {
        get;
        set;
    }
    Vector2 GetRoomPosOnGrid(float mult = -1);
}
