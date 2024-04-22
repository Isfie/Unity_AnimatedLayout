using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GridLayout : AnimatedLayout{
    [Header("Layout Properties")]
    [SerializeField] Vector3 rotation = Vector3.zero;
    [SerializeField] Vector3 scale = Vector3.one;

    [SerializeField] Vector2 cellSize = new Vector2(100f,100f);
    public StartAxis startAxis;
    [HideInInspector] public HorizontalChildAlignment horizontalChildAlignment;
    [HideInInspector] public VerticalChildAlignment verticalChildAlignment;
    [HideInInspector] public Constraint constraint;
    [HideInInspector] public int constraintCount;

    bool sortHorizontal = true;
    bool leftOrientationSide = true;

    protected override void Update(){
        if (transform.hasChanged){
            RequestUpdate();
            transform.hasChanged = false;
        }
        base.Update();
    }

    protected override void UpdateLayout(){
        int numColumns = constraintCount;

        if(constraint == Constraint.Flexible){
            if(startAxis == StartAxis.HorizontalUp || startAxis == StartAxis.HorizontalDown){
                numColumns = Mathf.FloorToInt(((RectTransform)transform).sizeDelta.x / cellSize.x);
            }else{
                numColumns = Mathf.FloorToInt(((RectTransform)transform).sizeDelta.y / cellSize.y);
            }
        }

        if(startAxis == StartAxis.HorizontalUp){
            sortHorizontal = true;
            leftOrientationSide = false;
        }else if(startAxis == StartAxis.HorizontalDown){
            sortHorizontal = true;
            leftOrientationSide = true;
        }else if(startAxis == StartAxis.VerticalLeft){
            sortHorizontal = false;
            leftOrientationSide = true;
        }else{
            sortHorizontal = false;
            leftOrientationSide = false;
        }

        if(numColumns == 0){
            numColumns = 1;
        }

        int numRows = Mathf.FloorToInt(transform.childCount / numColumns);

        
        foreach(Transform transform in childrenProperties.Keys){
            //Child Position
            float xPos = 0;
            float yPos = 0;
            float zPos = 0;

            if(sortHorizontal){
                xPos = (-((numColumns - 1) * (cellSize.x / 2))) + ((transform .GetSiblingIndex()% numColumns) * cellSize.x);
                if(horizontalChildAlignment == HorizontalChildAlignment.CenterLeft || horizontalChildAlignment == HorizontalChildAlignment.CenterRight){
                    if(transform.GetSiblingIndex() > (numColumns*numRows)-1){
                        xPos = (-(((this.transform.childCount - 1) % numColumns) * ((cellSize.x) / 2))) + ((transform.GetSiblingIndex() % numColumns) * (cellSize.x));
                    }
                }
                yPos = ((Mathf.FloorToInt((this.transform.childCount -1)/numColumns)) * ((cellSize.y) / 2)) - (Mathf.FloorToInt(transform.GetSiblingIndex() / numColumns) * (cellSize.y));
                if(leftOrientationSide){
                    yPos = -yPos;
                }

                if(horizontalChildAlignment == HorizontalChildAlignment.CenterRight || horizontalChildAlignment == HorizontalChildAlignment.Right){
                    xPos = -xPos;
                }
            }else{
                yPos = (-((numColumns - 1) * (cellSize.y / 2))) + ((transform.GetSiblingIndex() % numColumns) * cellSize.y);
                if(verticalChildAlignment == VerticalChildAlignment.CenterUp || verticalChildAlignment == VerticalChildAlignment.CenterDown){
                    if(transform.GetSiblingIndex() > (numColumns*numRows)-1){
                        yPos = (-(((this.transform.childCount - 1) % numColumns) * (cellSize.y / 2))) + ((transform.GetSiblingIndex() % numColumns) * cellSize.y);
                    }
                }
                xPos = (Mathf.FloorToInt((this.transform.childCount -1)/numColumns) * (cellSize.x / 2)) - (Mathf.FloorToInt(transform.GetSiblingIndex() / numColumns) * (cellSize.x));
                if(leftOrientationSide){
                    xPos = -xPos;
                }

                if(verticalChildAlignment == VerticalChildAlignment.CenterUp || verticalChildAlignment == VerticalChildAlignment.Up){
                    yPos = -yPos;
                }
            }

            //Child Scale
            float xScale = scale.x;
            float yScale = scale.y;
            float zScale = scale.z;

            //Child Rotation
            float xRot = rotation.x;
            float yRot = rotation.y;
            float zRot = rotation.z;

            childrenProperties[transform].position = new Vector3(xPos, yPos, zPos);
            childrenProperties[transform].rotation = new Vector3(xRot, yRot, zRot);
            childrenProperties[transform].scale = new Vector3(xScale, yScale, zScale);
        }
    }

    public enum StartAxis{
        HorizontalUp,
        HorizontalDown,
        VerticalLeft,
        VerticalRight
    }

    public enum HorizontalChildAlignment{
        Left,
        CenterLeft,
        CenterRight,
        Right
    }

    public enum VerticalChildAlignment{
        Up,
        CenterUp,
        CenterDown,
        Down
    }

    public enum Constraint{
        Flexible,
        FixedCount
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(GridLayout))]
public class GridLayoutEditor : Editor{
    public override void OnInspectorGUI(){
        DrawDefaultInspector();

        GridLayout gridLayout = (GridLayout)target;

        if(gridLayout.startAxis == GridLayout.StartAxis.HorizontalDown || gridLayout.startAxis == GridLayout.StartAxis.HorizontalUp){
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("     Child Alignment", GUILayout.MaxWidth(EditorGUIUtility.labelWidth));
            gridLayout.horizontalChildAlignment = (GridLayout.HorizontalChildAlignment)EditorGUILayout.EnumPopup(gridLayout.horizontalChildAlignment);
            EditorGUILayout.EndHorizontal();
        }else{
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("     Child Alignment", GUILayout.MaxWidth(EditorGUIUtility.labelWidth));
            gridLayout.verticalChildAlignment = (GridLayout.VerticalChildAlignment)EditorGUILayout.EnumPopup(gridLayout.verticalChildAlignment);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Constraint", GUILayout.MaxWidth(EditorGUIUtility.labelWidth));
        gridLayout.constraint = (GridLayout.Constraint)EditorGUILayout.EnumPopup(gridLayout.constraint);
        EditorGUILayout.EndHorizontal();

        if(gridLayout.constraint != GridLayout.Constraint.Flexible){
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("     Constraint Count", GUILayout.MaxWidth(EditorGUIUtility.labelWidth));
            gridLayout.constraintCount = EditorGUILayout.IntField(gridLayout.constraintCount);
            EditorGUILayout.EndHorizontal();
        }

        if (GUI.changed){
            gridLayout.RequestUpdate();
        }
    }
}
#endif