using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public abstract class AnimatedLayoutChild : MonoBehaviour{
    protected AnimatedLayout animatedLayout;
    protected AnimatedLayout.ChildProperties childProperties;

    public virtual void SetAnimatedLayout(AnimatedLayout animatedLayout){
        this.animatedLayout = animatedLayout;
    }

    public virtual void SetChildProperties(AnimatedLayout.ChildProperties childProperties){
        this.childProperties = childProperties;
    }

    private void OnValidate(){
        if(animatedLayout == null) {
            return;
        }
        animatedLayout.RequestUpdate();
    }

    private void OnTransformParentChanged(){
        AnimatedLayout animatedLayout = transform.parent.GetComponent<AnimatedLayout>();
        if (animatedLayout == null){
            this.animatedLayout = null;
        }
    }

    public abstract void UpdateLayout();
    public abstract void SetState(string state, AnimatedLayout.ChildProperties childProperties);
    public virtual void ProcessAdditionalEffect(float deltaTime){}
}
