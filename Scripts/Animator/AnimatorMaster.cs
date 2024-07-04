using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class AnimatorMaster : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] public UnityEvent<AnimatorTransitionInfo> onTransition;
    [SerializeField] public UnityEvent<AnimatorStateInfo, AnimatorTransitionInfo> onStateChangeStart;
    [SerializeField] public UnityEvent<AnimatorStateInfo> onStateChangeEnd;

    bool inTransition = false;
    // Start is called before the first frame update
    void Start()
    {
        if (!animator) animator = GetComponent<Animator>();
        if (!animator)
        {
            Debug.LogError("Goodness! there's no animator! How silly!");
            this.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
		if (animator.IsInTransition(0))
		{
            InvokeOnTransition();

            CheckTransitionStart();
        }
		else
		{
            CheckTransitionEnd();
        }
    }

    void InvokeOnTransition()
	{
        if (onTransition.GetPersistentEventCount() > 0)
        {
            onTransition.Invoke(animator.GetAnimatorTransitionInfo(0));
        }
    }

    void CheckTransitionStart()
	{
        if (!inTransition)
        {
            inTransition = true;
            if (onStateChangeStart.GetPersistentEventCount() > 0)
            {
                onStateChangeStart.Invoke(animator.GetNextAnimatorStateInfo(0), animator.GetAnimatorTransitionInfo(0));
            }
        }
    }

    void CheckTransitionEnd()
	{
        if (inTransition)
        {
            inTransition = false;
            if (onStateChangeEnd.GetPersistentEventCount() > 0)
            {
                onStateChangeEnd.Invoke(animator.GetNextAnimatorStateInfo(0));
            }
        }
    }
}
