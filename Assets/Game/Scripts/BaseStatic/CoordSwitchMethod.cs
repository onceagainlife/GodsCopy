
using UnityEngine;

namespace HuaHaiLiKanHua
{
    /// <summary>
    /// 坐标转换,错误的Camera.main会导致位置偏移。(建议用第一场景的相机来显示ui)
    /// </summary>
    public static class CoordSwitchMethod 
    {
        /// <summary>
        /// 世界坐标(2d,3D,World Space CanvasUi)转换为ui坐标，canvas模式为ScreenSpace,World Space Canvas。
        /// 但不建议在ScreenSpace模式下使用，AI说是会打破布局，我还没测试过
        /// </summary>
        /// <param name="enterPos">需要转化的坐标</param>
        /// <param name="uiTrans">需要更新位置的ui物体</param>
        public static void WorldToUI_ScreenSpace(Vector3 enterPos, Transform uiTrans, Camera uiCamera = null)
        {
            Vector3 vec = new Vector3();

            Vector3 pos = CoordinateConverter.WorldToScreenPoint(enterPos);
            vec = CoordinateConverter.ScreenToUIWorldPoint(uiTrans.parent.GetComponent<RectTransform>(), pos, uiCamera);
            uiTrans.GetComponent<RectTransform>().position = vec;

        }

        /// <summary>
        /// 世界坐标(3D,2D，ui)转换为UGUI局部坐标,保持z轴,canvas模式为WorldSpace
        /// 直接更新ui物体的位置，不返回坐标
        /// </summary>
        /// <param name="worldPosition">需要转化的坐标</param>
        /// <param name="uiRt">需要更新位置的ui物体</param>
        public static void WorldToUI_WorldSpace(Vector3 worldPosition, Transform uiRt, Camera cameraMain = null, Camera uiCamera = null)
        {
            Vector3 vec = new Vector3();

            cameraMain = cameraMain ?? Camera.main;
            uiCamera = uiCamera ?? Camera.main;

            vec = CoordinateConverter.WorldToUILocalPoint(worldPosition, uiRt.parent.GetComponent<RectTransform>(), cameraMain, uiCamera);

            uiRt.localPosition = new Vector3(
        vec.x,
       vec.y,
        uiRt.localPosition.z
    );
            //test = false;
        }

        /// <summary>
        /// 世界坐标转换为某个 3D/2D 物体的局部坐标系下的位置,同时更新物体的位置，保持z轴
        /// </summary>
        /// <param name="enterPos">需要转化的坐标</param>
        /// <param name="trans">需要更新的3D物体的坐标</param>
        public static void WorldToParent(Vector3 enterPos, Transform trans)
        {
            Vector3 vec = new Vector3();

            vec = trans.parent.transform.InverseTransformPoint(enterPos);

            Vector3 exitPos=new Vector3(vec.x,vec.y, trans.localPosition.z);

            trans.localPosition = exitPos;
        }
    }
    

    /// <summary>
    /// 坐标转换器
    /// </summary>
    public static class CoordinateConverter
    {
       

        /// <summary>
        /// 世界坐标(3D,2D，ui)转换为屏幕坐标,再转换为ui局部坐标
        /// 当源是3D或者2D时，应该传入世界坐标相机。ui时传入绑定的相机
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="parentRT"></param>
        /// <param name="worldPosition"></param>
        /// <param name="sourceUiCamera">源UI相机</param>
        /// <param name="targetUiCamera">目标UI相机</param>
        public static Vector2 WorldToUI_LocalPosition(RectTransform parentRT, Vector2 worldPosition, Camera sourceUiCamera = null, Camera targetUiCamera = null)
        {
            Camera cam = sourceUiCamera ?? Camera.main;
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldPosition);
            Camera targetCam = targetUiCamera ?? Camera.main;
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
    parentRT, // 目标UI所在的父级面板
    screenPos,
    targetCam,
        out localPoint
    );
            return localPoint;
        }

        /// <summary>
        /// 世界坐标转换为屏幕坐标
        /// 当调用者​​不提供​​此参数（Camera）时，默认值为 null
        /// </summary>
        public static Vector2 WorldToScreenPoint(Vector3 worldPoint, Camera camera = null)
        {
            camera = camera ?? Camera.main;
            return camera.WorldToScreenPoint(worldPoint);
        }

        /// <summary>
        /// 屏幕坐标转换为世界坐标
        /// </summary>
        public static Vector3 ScreenToWorldPointZPlane(Vector2 screenPoint, float targetZ, Camera camera = null)
        {
            camera = camera ?? Camera.main;

            // 计算射线
            Ray ray = camera.ScreenPointToRay(screenPoint);

            // 计算射线参数t，使得点在Z=targetZ平面上
            // 射线方程: p = origin + t * direction
            // 我们需要 p.z = targetZ
            float t = (targetZ - ray.origin.z) / ray.direction.z;

            // 检查是否合理
            if (Mathf.Abs(t) > 10000 || float.IsNaN(t) || float.IsInfinity(t))
            {
                Debug.LogWarning($"计算失败: origin.z={ray.origin.z}, direction.z={ray.direction.z}");
                return Vector3.zero;
            }

            return ray.origin + ray.direction * t;
        }

        /// <summary>
        /// UI世界坐标转换为屏幕坐标
        /// </summary>
        public static Vector2 UIWorldToScreenPoint(Vector3 worldPoint, Camera uiCamera)
        {
            return RectTransformUtility.WorldToScreenPoint(uiCamera, worldPoint);
        }


        /// <summary>
        /// 屏幕坐标转换为UGUI世界坐标
        /// </summary>
        /// <param name="rt">根据使用场景不同，rt可以是：
        ///​​父级 RectTransform​​：转换到父容器的坐标系
        ///​​Canvas 的 RectTransform​​：转换到整个画布的坐标系
        ///​​UI 元素自身的 RectTransform​​：转换到自身坐标系,ps这种情况只能用来处理子物体</param>
        /// <param name="screenPoint"></param>
        /// <param name="uiCamera"></param>
        /// <returns></returns>
        public static Vector3 ScreenToUIWorldPoint(RectTransform rt, Vector2 screenPoint, Camera uiCamera=null)
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, screenPoint, uiCamera, out Vector3 worldPos);
            return worldPos;
        }

        /// <summary>
        /// 屏幕坐标转换为UGUI局部坐标
        /// </summary>
        public static Vector2 ScreenToUILocalPoint(RectTransform parentRT, Vector2 screenPoint, Camera uiCamera)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRT, screenPoint, uiCamera, out Vector2 localPos);
            return localPos;
        }

        /// <summary>
        /// 世界坐标转换为UGUI局部坐标
        /// </summary>
        public static Vector2 WorldToUILocalPoint(Vector3 worldPoint, RectTransform parentRT, Camera worldCamera, Camera uiCamera)
        {
            // 先将世界坐标转换为屏幕坐标
            Vector3 screenPoint = WorldToScreenPoint(worldPoint, worldCamera);

            //if (screenPoint.z < 0)
            //{
            //    // 处理点在相机后方的情况
            //    screenPoint = HandleBehindCamera(screenPoint, worldCamera);
            //}

            // 再将屏幕坐标转换为UI局部坐标
            return ScreenToUILocalPoint(parentRT, screenPoint, uiCamera);
        }

        private static Vector3 HandleBehindCamera(Vector3 screenPoint, Camera camera)
        {
            // 转换为视口坐标
            Vector3 viewportPoint = camera.ScreenToViewportPoint(screenPoint);

            // 计算到屏幕中心的偏移
            Vector3 centerOffset = new Vector3(0.5f, 0.5f, 0) - viewportPoint;

            // 确定主要偏移方向
            if (Mathf.Abs(centerOffset.x) > Mathf.Abs(centerOffset.y))
            {
                // X轴偏移更大，移动到左右边缘
                viewportPoint.x = centerOffset.x > 0 ? 0 : 1;
                viewportPoint.y = Mathf.Clamp01(viewportPoint.y);
            }
            else
            {
                // Y轴偏移更大，移动到上下边缘
                viewportPoint.y = centerOffset.y > 0 ? 0 : 1;
                viewportPoint.x = Mathf.Clamp01(viewportPoint.x);
            }

            viewportPoint.z = 0; // 屏幕平面
            return camera.ViewportToScreenPoint(viewportPoint);
        }
    }
}


