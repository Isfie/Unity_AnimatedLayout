using System;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public abstract class AnimatedLayout : MonoBehaviour{
    [Header("Animation Properties")]
    [SerializeField] protected float transitionDelay;
    [SerializeField] protected bool animatePosition;
    [SerializeField] protected bool animateRotation;
    [SerializeField] protected bool animateScale;

    [Header("CharacterLayoutChild Properties")]
    public bool updateChildrenLayout;

    protected Dictionary<Transform, ChildProperties> childrenProperties = new Dictionary<Transform, ChildProperties>();
    private bool requestUpdate = false;

    public class ChildProperties{
        public readonly Transform transform;
        public Vector3 position;
        public float positionDelay;
        public Vector3 rotation;
        public float rotationDelay;
        public Vector3 scale;
        public float scaleDelay;

        public ChildProperties(Transform transform){
            this.transform = transform;
            position = transform.localPosition;
            rotation = transform.localEulerAngles;
            scale = transform.localScale;
        }
    }

    private class ChildPropertiesExtended : ChildProperties{
        public Vector3 positionDistance;
        public Vector3 rotationDistance;
        public Vector3 scaleDistance;
        public Vector3 rotationPosition;

        public ChildPropertiesExtended(Transform transform) : base(transform){
            rotationPosition = transform.localEulerAngles;
            if(rotationPosition.x >= 180f){
                rotationPosition.x -= 360f;
            }
            if(rotationPosition.y >= 180f){
                rotationPosition.y -= 360f;
            }
            if(rotationPosition.z >= 180f){
                rotationPosition.z -= 360f;
            }
        }

        public ChildPropertiesExtended(ChildProperties childProperties) : base(childProperties.transform){
            position = childProperties.position;
            positionDelay = childProperties.positionDelay;
            rotation = childProperties.rotation;
            rotationPosition = childProperties.transform.localEulerAngles;
            if(rotationPosition.x >= 180f){
                rotationPosition.x -= 360f;
            }
            if(rotationPosition.y >= 180f){
                rotationPosition.y -= 360f;
            }
            if(rotationPosition.z >= 180f){
                rotationPosition.z -= 360f;
            }
            rotationDelay = childProperties.rotationDelay;
            scale = childProperties.scale;
            scaleDelay = childProperties.scaleDelay;
        }
    }

    protected virtual void Start(){
        ResetChildrenProperties();
        RequestUpdate();
    }

    protected virtual void Update(){
        if(!requestUpdate){
            return;
        }

        requestUpdate = false;
        foreach(Transform transform in childrenProperties.Keys){
            if(childrenProperties[transform] is ChildPropertiesExtended){
                ChildPropertiesExtended childProperties = (ChildPropertiesExtended)childrenProperties[transform];

                if (animatePosition){
                    if (childProperties.transform.localPosition != childProperties.position){
                        Vector3 step = (childProperties.positionDistance / childProperties.positionDelay) * Time.deltaTime;

                        if((childProperties.position - childProperties.transform.localPosition).magnitude <= step.magnitude){
                            childProperties.transform.localPosition = childProperties.position;
                        }else{
                            childProperties.transform.localPosition += step;
                            requestUpdate = true;
                        }
                    }
                }

                if(animateRotation){
                    if (childProperties.rotationPosition != childProperties.rotation){
                        Vector3 step = (childProperties.rotationDistance / childProperties.rotationDelay) * Time.deltaTime;

                        if((childProperties.rotation - childProperties.rotationPosition).magnitude <= step.magnitude){
                            childProperties.transform.localEulerAngles = childProperties.rotation;
                            childProperties.rotationPosition = childProperties.rotation;
                        }else{
                            childProperties.transform.localEulerAngles += step;
                            childProperties.rotationPosition += step;
                            requestUpdate = true;
                        }
                    }
                }

                if(animateScale){
                    if(childProperties.transform.localScale != childProperties.scale){
                        Vector3 step = (childProperties.scaleDistance / childProperties.scaleDelay) * Time.deltaTime;

                        if((childProperties.scale - childProperties.transform.localScale).magnitude <= step.magnitude){
                            childProperties.transform.localScale = childProperties.scale;
                        }else{
                            childProperties.transform.localScale += step;
                            requestUpdate = true;
                        }
                    }
                }

                ProcessAdditionalEffect(transform, Time.deltaTime);

                if(childrenProperties[transform].transform.GetComponent<AnimatedLayoutChild>()){
                    childrenProperties[transform].transform.GetComponent<AnimatedLayoutChild>().ProcessAdditionalEffect(Time.deltaTime);
                }
            }
        }
    }

    protected virtual void ProcessAdditionalEffect(Transform transform, float deltaTime){}

    public virtual void RequestUpdate(){
        ResetTransitionDelay();
        UpdateLayout();
        if (updateChildrenLayout){
            UpdateChildLayout();
        }
        SetDistance();

        requestUpdate = true;
    }

    private void ResetTransitionDelay(){
        foreach(Transform transform in childrenProperties.Keys){
            childrenProperties[transform].positionDelay = transitionDelay;
            childrenProperties[transform].rotationDelay = transitionDelay;
            childrenProperties[transform].scaleDelay = transitionDelay;
        }
    }

    protected abstract void UpdateLayout();

    private void UpdateChildLayout(){
        foreach(Transform transform in childrenProperties.Keys){
            if(childrenProperties[transform].transform.GetComponent<AnimatedLayoutChild>()){
                childrenProperties[transform].transform.GetComponent<AnimatedLayoutChild>().UpdateLayout();
            }
        }
    }

    private void SetDistance(){
        foreach(Transform transform in childrenProperties.Keys){
            if(childrenProperties[transform] is not ChildPropertiesExtended){
                ChildPropertiesExtended childPropertiesExtended = new ChildPropertiesExtended(childrenProperties[transform]);

                childrenProperties[transform] = childPropertiesExtended;
            }
            ChildPropertiesExtended childProperties = (ChildPropertiesExtended)childrenProperties[transform];

            childProperties.positionDistance = childProperties.position - childProperties.transform.localPosition;
            childProperties.rotationDistance = childProperties.rotation - childProperties.rotationPosition;
            childProperties.scaleDistance = childProperties.scale - childProperties.transform.localScale;
        }
    }

    public virtual void SetChildrenState(string state, ChildProperties childProperties){
        foreach(Transform transform in childrenProperties.Keys){
            if(childrenProperties[transform].transform.GetComponent<AnimatedLayoutChild>()){
                childrenProperties[transform].transform.GetComponent<AnimatedLayoutChild>().SetState(state, childProperties);
            }
        }
    }

    public virtual void SetChildState(Transform transform, string state, ChildProperties childProperties){
        if(transform == null){
            return;
        }

        if(!childrenProperties.ContainsKey(transform)){
            return;
        }

        if(transform.GetComponent<AnimatedLayoutChild>()){
            transform.GetComponent<AnimatedLayoutChild>().SetState(state, childProperties);
        }
    }

    protected virtual void OnTransformChildrenChanged(){
        if (transform.childCount == childrenProperties.Count){
            RequestUpdate();
            return;
        }

        ResetChildrenProperties();
        RequestUpdate();
    }

    private void ResetChildrenProperties(){
        childrenProperties.Clear();
        for (int i = 0; i < transform.childCount; i++){
            Transform childTransform = transform.GetChild(i);
            ChildPropertiesExtended childProperties = new ChildPropertiesExtended(childTransform);
            childrenProperties.Add(childTransform, childProperties);
            AnimatedLayoutChild animatedLayoutChild = childTransform.GetComponent<AnimatedLayoutChild>();
            if(animatedLayoutChild != null){
                animatedLayoutChild.SetAnimatedLayout(this);
                animatedLayoutChild.SetChildProperties(childProperties);
            }
        }
    }

    protected virtual void OnValidate(){
        if(transitionDelay == 0){
            return;
        }

        RequestUpdate();
    }

    protected virtual void OnEnable(){
        RequestUpdate();
    }
}