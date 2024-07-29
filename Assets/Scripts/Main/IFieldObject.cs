using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFieldObject 
{
    public bool IsActive { get; set; }

    public void Initialize();
    public void Pop();
}
