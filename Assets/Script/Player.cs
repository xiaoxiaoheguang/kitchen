using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour,IKitchenObjectParent
{
    public static Player instance { get; private set; }
    //public static PlayControl instanceField;
    //public static PlayControl GetInstanceFeild() {
    //    return instanceField;
    //}
    //public static void SetInstanceFeild(PlayControl instance)
    //{
    //    instanceField = instance;
    //}

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private GameInput gameInput;

    [SerializeField] private Transform kitchenObjectHoldPoint;
    [SerializeField] private KitchenObject KitchenObject;

    private BaseCounter selectCounter;

    public event EventHandler OnPickSomething;
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }


    private bool isWalking = false;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("more instence");
        }
        instance = this;
    }
    void Start()
    {
        gameInput.OnInteraction += GameInput_OnInteraction;
        gameInput.OnInteractionAlternate += GameInput_OnInteractionAlternate;
    }

    private void GameInput_OnInteractionAlternate(object sender, EventArgs e)
    {
        if(!GameManager.Instance.IsGamePlaying())
        {
            return;
        }
        if (selectCounter != null)
        {
            selectCounter.InteractAlternate(this);
        }
    }

    private void GameInput_OnInteraction(object sender, System.EventArgs e)
    {
        if (!GameManager.Instance.IsGamePlaying())
        {
            return;
        }
        if (selectCounter != null)
        {
            selectCounter.Interact(this);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        HandleMovement();
        HandleInteractions();
    }

    public bool IsWalking()
    {
        return isWalking;
    }

    private Vector3 lastInteractionDir;

    private void SetSelectCounter(BaseCounter baseCounter)
    {
        selectCounter = baseCounter;
        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs
        {
            selectedCounter = selectCounter,
        });
    }
    public void HandleInteractions()
    {
        Vector2 InputVector = gameInput.GetMovement_Normalized_Vector2();
        Vector3 movedir = new Vector3(InputVector.x, 0f, InputVector.y);

        if (movedir != Vector3.zero)
        {
            lastInteractionDir = movedir;
        }

        float interactDistance = 1f;
        if (Physics.Raycast(transform.position, lastInteractionDir, out RaycastHit raycastHit, interactDistance))
        {
            if (raycastHit.transform.TryGetComponent(out BaseCounter basecounter))
            {
                if (basecounter != selectCounter)
                {
                    //selectCounter = clearCounter;
                    //OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs
                    //{
                    //    selectedCounter = selectCounter,
                    //});
                    SetSelectCounter(basecounter);
                }
            }
            else
            {
               SetSelectCounter(null);
            }
        }
        else
        {

            SetSelectCounter(null);
        }
    }
    public void HandleMovement()
    {
        Vector2 InputVector = gameInput.GetMovement_Normalized_Vector2();
        Vector3 movedir = new Vector3(InputVector.x, 0f, InputVector.y);
        //这里通过 gameInput 获取玩家输入的移动向量（二维）
        //然后将其转换为三维的 Vector3，其中 y 轴方向设为 0f，因为这是一个基于水平面的移动。

        float moveDistance = moveSpeed * Time.deltaTime;
        float playRadius = .6f;
        float playHeight = 2f;
        //moveDistance 是根据 moveSpeed 和 Time.deltaTime 计算得出的，确保移动的距离与帧率无关。
        //playRadius 和 playHeight 用于描述玩家角色的体积大小，稍后用于胶囊体的碰撞检测。

        bool canmove = !Physics.CapsuleCast(transform.position,
            transform.position + playHeight * Vector3.up,
            playRadius,
            movedir,
            moveDistance);
        //投射胶囊形状的射线判断下一个时刻到达位置是否有物体

        if (!canmove)
        {
            Vector3 movedirX = new Vector3(movedir.x, 0, 0);

            bool canmoveX = Mathf.Abs(movedir.x)>0.5f
                && !Physics.CapsuleCast(
                transform.position,
                transform.position + playHeight * Vector3.up,
                playRadius,
                movedirX,
                moveDistance);

            if (canmoveX)
            {
                transform.position += movedirX * Time.deltaTime * moveSpeed;
            }
            else
            {
                Vector3 movedirZ = new Vector3(0, 0, movedir.z);

                bool canmoveZ = Mathf.Abs(movedir.z) > 0.5f
                    && !Physics.CapsuleCast(
                    transform.position,
                    transform.position + playHeight * Vector3.up,
                    playRadius,
                    movedirZ,
                    moveDistance);

                
                if (canmoveZ)
                {
                    transform.position += movedirZ * Time.deltaTime * moveSpeed;
                }
            }
        }
        else
        {
            transform.position += movedir * Time.deltaTime * moveSpeed;
        }

        isWalking = movedir != Vector3.zero;

        float rotationSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, movedir, Time.deltaTime * rotationSpeed);

        //Debug.Log(InputVector);
    }

    public Transform GetKitchenObjectFollowTransform()
    {
        return kitchenObjectHoldPoint;
    }
    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.KitchenObject = kitchenObject;

        if (this.KitchenObject != null)
        {
            OnPickSomething?.Invoke(this, EventArgs.Empty);
        }
    }
    public KitchenObject GetKitchenObject()
    {
        return KitchenObject;
    }
    public void ClearKitchenObject()
    {
        KitchenObject = null;
    }
    public bool HasKitchenObject()
    {
        return (KitchenObject != null);
    }
}
