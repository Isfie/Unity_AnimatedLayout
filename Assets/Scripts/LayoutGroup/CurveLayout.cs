using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveLayout : AnimatedLayout{
    [Header("Layout Properties")]
    [SerializeField] float spacing;
    [SerializeField] float curveRadius;
    [SerializeField] Vector3 angleTilted;
    [SerializeField] Vector3 scale = Vector3.one;

    protected override void UpdateLayout(){
        float iCenter = (childrenProperties.Count - 1f) / 2f;
        foreach(Transform transform in childrenProperties.Keys){
            //Child Position
            float xPos = (-((childrenProperties.Count - 1) * ((spacing) / 2))) + (transform.GetSiblingIndex() * (spacing));
            float yPos = 0;
            float zPos = 0;
            if(curveRadius != 0){
                float angle = spacing * Mathf.Abs((float)transform.GetSiblingIndex() - iCenter) / curveRadius;
                float xOffSet = curveRadius * Mathf.Sin(angle);
                float yOffSet = curveRadius * Mathf.Cos(angle);

                if((float)transform.GetSiblingIndex() < iCenter){
                    xPos = -xOffSet;
                    yPos = yOffSet - curveRadius;
                }else if((float)transform.GetSiblingIndex() > iCenter){
                    xPos = xOffSet;
                    yPos = yOffSet - curveRadius;
                }
            }

            //Child Scale
            float xScale = scale.x;
            float yScale = scale.y;
            float zScale = scale.z;

            //Child Rotation
            float xRot = -((-((childrenProperties.Count - 1) * ((angleTilted.x) / 2))) + (transform.GetSiblingIndex() * (angleTilted.x)));
            float yRot = -((-((childrenProperties.Count - 1) * ((angleTilted.y) / 2))) + (transform.GetSiblingIndex() * (angleTilted.y)));
            float zRot = -((-((childrenProperties.Count - 1) * ((angleTilted.z) / 2))) + (transform.GetSiblingIndex() * (angleTilted.z)));

            childrenProperties[transform].position = new Vector3(xPos, yPos, zPos);
            childrenProperties[transform].rotation = new Vector3(xRot, yRot, zRot);
            childrenProperties[transform].scale = new Vector3(xScale, yScale, zScale);
        }
    }
}
