using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayAnimator :NetworkBehaviour
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
        if (!IsOwner)
        {
            return;
        }
        animator.SetBool(IsWalk, control.IsWalking());
    }
}
