using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeletons : Enemy, IFacingMover
{
    [Header("Set in Inspector: Skeletons")]
    public int speed = 2;
    public float timeThinkMin = 1f;
    public float timeThinkMax = 4f;

    [Header("Set Dynamically: Skeletons")]
    public int facing = 0;
    public float timeNextDecision = 0;

    private InRoom inRm;

    protected override void Awake()
    {
        base.Awake();
        inRm = GetComponent<InRoom>();
    }

    override protected void Update()
    {
        base.Update();
        if (knockback)
            return;
        if (Time.time >= timeNextDecision)
            DecideDirection();

        // rigid наследованно из Enemy и инициализируется в Enemy.Awake
        rigid.velocity = directions[facing] * speed;
    }

    void DecideDirection()
    {
        facing = Random.Range(0, 4);
        timeNextDecision = Time.time + Random.Range(timeThinkMin, timeThinkMax);
    }

    // Реализация интерфейса IFacingMover
    public int GetFacing()
    {
        return facing;
    }

    public bool Moving => true;

    public float GetSpeed()
    {
        return speed;
    }

    public float GridMult => inRm.gridMult;

    public Vector2 RoomPos
    { 
        get => inRm.RoomPos; 
        set => inRm.RoomPos = value; 
    }
    public Vector2 RoomNum 
    { 
        get => inRm.RoomNum; 
        set => inRm.RoomNum = value; 
    }

    public Vector2 GetRoomPosOnGrid(float mult = -1)
    {
        return inRm.GetRoomPosOnGrid(mult);
    }
}
