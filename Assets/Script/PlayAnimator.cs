using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAnimator : MonoBehaviour
{
    private const string IsWalk = "IsWalking";
    private Animator animator;
    [SerializeField] private Player control;
    private void Awake()
    {
        animator = GetComponent<Animator>();

    }

    private void Update()
    {
        animator.SetBool(IsWalk, control.IsWalking());
    }
}
