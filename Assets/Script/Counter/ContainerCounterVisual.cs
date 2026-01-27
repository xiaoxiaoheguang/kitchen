using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerCounterVisual : MonoBehaviour
{
    [SerializeField] private ContainerCounter counter;
    private Animator animator;
    private const string OPEN_CLOSE = "OpenClose";
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        counter.OnPlayerGrabbedObject += Counter_OnPlayerGrabbedObject;
    }

    private void Counter_OnPlayerGrabbedObject(object sender, System.EventArgs e)
    {
        animator.SetTrigger(OPEN_CLOSE);
    }
}
