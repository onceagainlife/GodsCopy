using HuaHaiLiKanHua;
using HuaHaiLiKanHua.Gods;
using StarForce;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : UGuiForm
{
    [Tooltip("属性数据和ui显示")]
    public PlayerAttributeText playerAttributeText;
    [Tooltip("资源数据和ui显示")]
    public ResourcesText resourcesText;
    [HideInInspector]
    public PlayerEntity playerEntity;

    [Tooltip("hp")]
    public Slider sliderHp;

    [Tooltip("全局数值管理")]
    public GlobalValueManager globalValueManager = new();

    public SkillBuffUIController skillBuffUIController;

    private void Awake()
    {
       
    }
}
