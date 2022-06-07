using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorSlave : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] AnimatorMaster master;
    // Start is called before the first frame update
    void Start()
    {
        if (!animator) animator = GetComponent<Animator>();
        if (!animator)
        {
            Debug.LogError("Goodness! there's no animator! How silly!");
            this.enabled = false;
        }

        if (!master) master = GetComponentInParent<AnimatorMaster>();
        if (!master)
        {
            Debug.LogError("Goodness! there's no animator master! How silly!");
            this.enabled = false;
        }
		else
		{
            master.onStateChangeStart.AddListener(TransitionStart);
		}
    }

    void TransitionStart(AnimatorStateInfo state, AnimatorTransitionInfo transition)
    {
        animator.CrossFade(state.fullPathHash, transition.duration,0,state.normalizedTime,transition.normalizedTime);
    }

}
