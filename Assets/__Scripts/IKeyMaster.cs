using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IKeyMaster
{
    int KeyCount 
    { 
        get; 
        set; 
    }
    int GetFacing();

}
