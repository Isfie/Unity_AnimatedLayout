using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedLayout : AnimatedLayout{
    [Header("Layout Properties")]
    [SerializeField] Vector3 position = Vector3.zero;
    [SerializeField] Vector3 rotation = Vector3.zero;
    [SerializeField] Vector3 scale = Vector3.one;

    protected override void UpdateLayout(){
        foreach(Transform transform in childrenProperties.Keys){
            //Child Position
            float xPos = position.x;
            float yPos = position.y;
            float zPos = position.z;

            //Child Scale
            float xScale = scale.x;
            float yScale = scale.y;
            float zScale = scale.z;

            //Child Rotation
            float xRot = rotation.x;
            float yRot = rotation.y;
            float zRot = rotation.z;

            //Child Transition Speed
            childrenProperties[transform].position = new Vector3(xPos, yPos, zPos);
            childrenProperties[transform].rotation = new Vector3(xRot, yRot, zRot);
            childrenProperties[transform].scale = new Vector3(xScale, yScale, zScale);
        }
    }
}
