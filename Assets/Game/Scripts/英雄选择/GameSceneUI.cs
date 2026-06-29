using GameFramework.Event;
using HuaHaiLiKanHua;
using StarForce;
using UnityEngine;

namespace BackPackLike
{
    /// <summary>
    /// 游戏场景UI管理类
    /// </summary>

    public class GameSceneUI : MonoBehaviour
    {
        [Header("=== Canvas结构引用 ===")]

        // 注意：这里建议使用 GameObject 或 Transform，方便控制显隐
        [Tooltip("游戏场景主画布根节点")]
        [SerializeField] private GameObject GameSceneCanvas;

        [Tooltip("UI网格状态渲染区域")]
        [SerializeField] private GameObject UIGridStateRenderCanvas;

        [Tooltip("玩家UI面板")]
        [SerializeField] private GameObject PlayerCanvas;

        [Tooltip("卡牌UI面板")]
        [SerializeField] private GameObject CardCanvas;

        // 对应图片底部的 GameMenuForm (通常用于英雄选择/菜单)
        [Tooltip("角色选择界面/菜单表单")]
        [SerializeField] private GameObject GameMenuForm;

        [Header("=== 其他UI ===")]

        [Tooltip("敌人棋盘")]
        [SerializeField] private GameObject EnemyArea;
        [Tooltip("玩家棋盘")]
        [SerializeField] private GameObject PlayerArea;
        [Tooltip("手牌区域")]
        [SerializeField] private GameObject handleArea;
        [Tooltip("商店")]
        [SerializeField]
        private GameObject Shop;


        #region 生命周期
        private void Awake()
        {
            // 订阅事件：传入事件Id和回调方法
            GameEntry.Event.Subscribe(StartGameEvent.EventId, OnStartGame);
            GameMenuForm.SetActive(true);
        }

        private void OnDestroy()
        {
             GameEntry.Event.Unsubscribe(StartGameEvent.EventId, OnStartGame);
        }
        #endregion
        public void OnStartGame(object sender, GameEventArgs e)
        {            
            //关闭英雄选择界面
            //开启游戏界面
            //游戏界面内部商店开启
            //开启玩家ui
            //玩家英雄实体化
            //商店数据初始化
            GameMenuForm.SetActive(false);
            GameSceneCanvas.SetActive(true);
            PlayerCanvas.SetActive(true);

            PlayerArea.SetActive(true);
            Shop.SetActive(true);
            handleArea.SetActive(true);
        }


        #region 自动绑定

        private void Reset()
        {
            AutoBindUI();
        }
        /// <summary>
        /// 自动绑定方法：利用反射或硬编码查找子物体
        /// </summary>
        private void AutoBindUI()
        {
            // 方式一：硬编码查找（性能最好，推荐）
            // 假设这些 Canvas 都是当前物体的直接子物体
            GameSceneCanvas = FindChildObject("GameSceneCanvas");
            UIGridStateRenderCanvas = FindChildObject("UIGrid状态渲染canvas"); // 注意：如果名字有空格或特殊字符，必须完全匹配
            PlayerCanvas = FindChildObject("PlayerCanvas");
            CardCanvas = FindChildObject("CardCanvas");
            GameMenuForm = FindChildObject("GameMenuForm");

            // 检查是否绑定成功，防止空引用报错
            if (GameSceneCanvas == null) Debug.LogError("未找到 GameSceneCanvas，请检查层级名称！");
            if (PlayerCanvas == null) Debug.LogError("未找到 PlayerCanvas，请检查层级名称！");
        }

        /// <summary>
        /// 辅助方法：查找直接子物体
        /// </summary>
        private GameObject FindChildObject(string name)
        {
            Transform child = transform.Find(name);
            return child != null ? child.gameObject : null;
        }
        #endregion
    

    }

}