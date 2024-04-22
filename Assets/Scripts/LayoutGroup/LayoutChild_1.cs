using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LayoutChild_1 : AnimatedLayoutChild, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler{
    [SerializeField] Canvas canvas;
    [SerializeField] StateProperties hover;
    [SerializeField] StateProperties hoverToLeft;
    [SerializeField] StateProperties hoverToRight;
    [SerializeField] StateProperties select;
    State stateFlag = new State();

    [Serializable]
    private class StateProperties{
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
    }

    private class State{
        int flag = 0;

        public bool Exist(int state){
            return flag == 0 ? false : (flag & state) != 0;
        }

        public void Add(int state){
            if(!Exist(state)){
                flag += state;
            }
        }

        public void AddIf(int state, int ifState){
            if(Exist(ifState)){
                Add(state);
            }
        }

        public void AddIfNot(int state, int ifState){
            if(!Exist(ifState)){
                Add(state);
            }
        }

        public void Remove(int state){
            if(Exist(state)){
                flag -= state;
            }
        }

        public void RemoveIf(int state, int ifState){
            if(Exist(ifState)){
                Remove(state);
            }
        }

        public void RemoveIfNot(int state, int ifState){
            if(!Exist(ifState)){
                Remove(state);
            }
        }

        public void Invert(int state){
            if(Exist(state)){
                flag -= state;
                return;
            }
            flag += state;
        }

        public void InvertIf(int state, int ifState){
            if(Exist(ifState)){
                Invert(state);
            }
        }

        public void InvertIfNot(int state, int ifState){
            if(!Exist(ifState)){
                Invert(state);
            }
        }

        public void Clear(){
            flag = 0;
        }
    }

    public void OnPointerEnter(PointerEventData eventData){
        if(animatedLayout == null){
            return;
        }

        animatedLayout.SetChildrenState("MouseEnter", childProperties);
        animatedLayout.RequestUpdate();
    }

    public void OnPointerExit(PointerEventData eventData){
        if(animatedLayout == null){
            return;
        }

        animatedLayout.SetChildrenState("MouseExit", childProperties);
        animatedLayout.RequestUpdate();
    }

    public void OnPointerDown(PointerEventData eventData){
        if(animatedLayout == null){
            return;
        }

        animatedLayout.SetChildrenState("MouseDown", childProperties);
        animatedLayout.RequestUpdate();
    }

    protected void Update(){
        if (stateFlag.Exist((int)STATE.DRAG)){
            if (Input.GetMouseButtonUp(0)){
                animatedLayout.SetChildrenState("MouseUp", childProperties);
                animatedLayout.RequestUpdate();
                return;
            }
        }
    }

    public override void UpdateLayout(){
        canvas.overrideSorting = false;
        canvas.sortingOrder = 0;

        if (stateFlag.Exist((int)STATE.DRAG)){
            return;
        }

        if (stateFlag.Exist((int)STATE.WAIT)){
            return;
        }

        if (stateFlag.Exist((int)STATE.HOVER)){
            childProperties.position += hover.position;
            childProperties.rotation += hover.rotation;
            childProperties.scale += hover.scale;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 1;
        }

        if (stateFlag.Exist((int)STATE.LEFT)){
            childProperties.position += hoverToLeft.position;
            childProperties.rotation += hoverToLeft.rotation;
            childProperties.scale += hoverToLeft.scale;
        }

        if (stateFlag.Exist((int)STATE.RIGHT)){
            childProperties.position += hoverToRight.position;
            childProperties.rotation += hoverToRight.rotation;
            childProperties.scale += hoverToRight.scale;
        }

        if (stateFlag.Exist((int)STATE.SELECT)){
            childProperties.position += select.position;
            childProperties.rotation += select.rotation;
            childProperties.scale += select.scale;
        }
    }

    enum STATE{
        HOVER = 1,
        SELECT = 2,
        RIGHT = 4,
        LEFT = 8,
        DRAG = 16,
        WAIT = 32,
        MOVED = 64
    }

    public override void SetState(string state, AnimatedLayout.ChildProperties childProperties){
        if (state == ""){
            stateFlag.Clear();
            return;
        }

        if (childProperties == null){
            return;
        }

        int callIndex = childProperties.transform.GetSiblingIndex();
        int myIndex = transform.GetSiblingIndex();

        if (callIndex != myIndex){
            if(state == "MouseEnter"){
                if (callIndex > myIndex){
                    stateFlag.Add((int)STATE.LEFT);
                }else if(callIndex < myIndex){
                    stateFlag.Add((int)STATE.RIGHT);
                }

                if (stateFlag.Exist((int)STATE.DRAG)){
                    transform.SetSiblingIndex(childProperties.transform.GetSiblingIndex());
                }

                return;
            }

            if(state == "MouseExit"){
                stateFlag.Remove((int)STATE.LEFT);
                stateFlag.Remove((int)STATE.RIGHT);

                stateFlag.AddIf((int)STATE.MOVED, (int)STATE.WAIT);
                return;
            }

            if(state == "MouseDown"){
                stateFlag.Add((int)STATE.WAIT);
                return;
            }

            if(state == "MouseUp"){
                stateFlag.Remove((int)STATE.WAIT);
                stateFlag.RemoveIfNot((int)STATE.SELECT, (int)STATE.MOVED);
                stateFlag.Remove((int)STATE.MOVED);
                return;
            }

            return;
        }

        if (state == "MouseEnter"){
            stateFlag.Add((int)STATE.HOVER);
            return;
        }

        if(state == "MouseExit"){
            stateFlag.Remove((int)STATE.HOVER);
            stateFlag.AddIf((int)STATE.MOVED, (int)STATE.DRAG);
            return;
        }

        if(state == "MouseDown"){
            stateFlag.Add((int)STATE.DRAG);
            return;
        }

        if(state == "MouseUp"){
            stateFlag.Remove((int)STATE.DRAG);
            stateFlag.InvertIfNot((int)STATE.SELECT, (int)STATE.MOVED);
            stateFlag.Remove((int)STATE.MOVED);
            return;
        }
    }
}
