using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 配送结果UI类，用于显示配送成功或失败的提示
/// </summary>
/// <remarks>
/// 监听 DeliveryManager 的成功和失败事件
/// 通过动画弹出显示配送结果
/// </remarks>
public class DeliveryResultUI : MonoBehaviour
{
    #region Constants

    /// <summary>
    /// 动画触发器参数名
    /// </summary>
    private const string POPUP = "Popup";

    #endregion

    #region Serialized Fields

    /// <summary>
    /// 背景图片组件
    /// </summary>
    [SerializeField] private Image backgroundImage;

    /// <summary>
    /// 图标图片组件
    /// </summary>
    [SerializeField] private Image iconImage;

    /// <summary>
    /// 消息文本组件
    /// </summary>
    [SerializeField] private TextMeshProUGUI messageText;

    /// <summary>
    /// 配送成功时的背景颜色
    /// </summary>
    [SerializeField] private Color successColor;

    /// <summary>
    /// 配送失败时的背景颜色
    /// </summary>
    [SerializeField] private Color failedColor;

    /// <summary>
    /// 配送成功时的图标
    /// </summary>
    [SerializeField] private Sprite successSprite;

    /// <summary>
    /// 配送失败时的图标
    /// </summary>
    [SerializeField] private Sprite failedSprite;

    #endregion

    #region Private Fields

    /// <summary>
    /// 动画控制器组件
    /// </summary>
    private Animator animator;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// 初始化组件引用
    /// </summary>
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// 订阅事件并初始化UI状态
    /// </summary>
    private void Start()
    {
        DeliveryManager.Instance.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
        DeliveryManager.Instance.OnRecipeFailed += DeliveryManager_OnRecipeFailed;

        gameObject.SetActive(false);
    }

    #endregion

    /// <summary>
    /// 处理配送失败事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">事件参数</param>
    private void DeliveryManager_OnRecipeFailed(object sender, System.EventArgs e)
    {
        ShowResult(false);
    }

    /// <summary>
    /// 处理配送成功事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">事件参数</param>
    private void DeliveryManager_OnRecipeSuccess(object sender, System.EventArgs e)
    {
        ShowResult(true);
    }

    /// <summary>
    /// 显示配送结果UI
    /// </summary>
    /// <param name="isSuccess">是否配送成功</param>
    private void ShowResult(bool isSuccess)
    {
        Debug.Log("显示配送结果UI: " + (isSuccess ? "成功" : "失败"));

        gameObject.SetActive(true);
        
        // 重置动画状态，确保触发器可以再次触发
        animator.ResetTrigger(POPUP);
        
        // 设置UI内容
        if (isSuccess)
        {
            backgroundImage.color = successColor;
            iconImage.sprite = successSprite;
            messageText.text = "DELIVERY\nSUCCESS";
        }
        else
        {
            backgroundImage.color = failedColor;
            iconImage.sprite = failedSprite;
            messageText.text = "DELIVERY\nFAILED";
        }
        

        Debug.Log("UI内容已设置，准备触发动画");
        // 触发动画
        animator.SetTrigger(POPUP);
        Debug.Log("动画触发器已设置. "+POPUP);
    }

    //todo: 只显示一次提交订单动画:六六六又是单词拼错

}