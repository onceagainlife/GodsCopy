using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

public class LitionTest : MonoBehaviour
{
    public LoopType loopType;
    public float timer = 1;
    private MotionHandle _currentMotion; // 用来持有动画


    public float bounceHeight = 50f;   // 弹跳幅度（Y轴位移）
    public float bounceTime = 0.4f;   // 弹跳时间
    public float fadeTime = 0.5f;     // 退场时间
    public float spinAngle = 720f;    // 退场旋转角度

    //public void PlayDeath()
    //{
    //    // 记录初始位置（如果要弹跳位移的话）
    //    Vector3 startPos = transform.localPosition;
    //    Vector3 endPos = startPos + new Vector3(0, bounceHeight, 0);

    //    // 记录初始旋转（如果要旋转退场）
    //    Vector3 startRot = transform.localEulerAngles;

    //    // 获取SpriteRenderer方便做淡出（如果用UITK/CanvasGroup自行替换）
    //    var sr = GetComponent<SpriteRenderer>();

    //    LSequence.Create()
    //        // === 阶段1：受击/弹跳反馈（通常用Scale弹跳或位置弹跳）===
    //        // 方式A：Y轴位移弹跳（像被击飞一下）
    //        .Append(
    //            LMotion.Create(startPos.y, endPos.y, bounceTime)
    //                .WithEase(Ease.OutBounce) // 关键：弹跳缓动
    //                .Bind(y => transform.localPosition = new Vector3(startPos.x, y, startPos.z))
    //        )
    //        // 可选：落回原位（如果要先弹上去再落下来可以加这一段，或者直接停在上面进下一阶段）
    //        // .Append(LMotion.Create(endPos.y, startPos.y, bounceTime * 0.8f).WithEase(Ease.InQuad).Bind(...))

    //        // === 阶段2：卡牌旋转+缩小+淡出（退场）===
    //        // 这里用 Join 让旋转、缩放、淡出同时进行
    //        .Append(
    //            LSequence.Create() // 嵌套一个并行序列演示Join用法
    //                .Join(LMotion.Create(startRot.y, startRot.y + spinAngle, fadeTime)
    //                    .WithEase(Ease.InBack) // 加速旋转
    //                    .BindToLocalEulerAnglesZ(transform)) // 如果是Z轴旋转，换成 BindToLocalEulerAnglesZ
    //                .Join(LMotion.Create(Vector3.one, Vector3.zero, fadeTime)
    //                    .WithEase(Ease.InQuad)
    //                    .BindToLocalScale(transform))
    //                .Join(LMotion.Create(1f, 0f, fadeTime)
    //                    .WithEase(Ease.Linear)
    //                    .Bind(alpha => {
    //                        if (sr) sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
    //                        // 如果是UI Image: img.color = ...
    //                    }))
    //                .Run()
    //        )

    //        .Run();
    //}

    public Transform targetPoint; // 拖一个空物体进来当目标点
    public float flyTime = 0.6f;

    public Vector3 flyDirection = new Vector3(-1, 1, 0); // 左上斜飞
    public float flyDistance = 5f;

    public void PlayDeath2()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + flyDirection.normalized * flyDistance;

        Vector3 startRot = transform.localEulerAngles;

        LSequence.Create()
            .Append(
                LSequence.Create()
                    .Join(
                        LMotion.Create(startPos, endPos, flyTime)
                            .WithEase(Ease.InOutQuad)
                            .Bind(pos => transform.position = pos)
                    )
                    .Join(
                        LMotion.Create(startRot.z, startRot.z + spinAngle, flyTime)
                            .WithEase(Ease.InBack)
                            .BindToLocalEulerAnglesZ(transform)
                    )
                    .Run()
            )

            .Run();
    }

    public void PlayDeath1()
    {
        Vector3 startPos = transform.localPosition;
        Vector3 endPos = targetPoint.position;
        Vector3 startRot = transform.localEulerAngles;

        LSequence.Create()

            // ✅ 一段搞定：弹跳 + 飞行 + 旋转
            .Append(
                LSequence.Create()
                    // 位置：抛物线飞向目标
                    .Join(
                        LMotion.Create(startPos, endPos, flyTime)
                            .WithEase(Ease.InOutQuad) // 关键：平滑斜飞
                            .Bind(pos => transform.localPosition = pos)
                    )
                    // 旋转：高速自旋
                    .Join(
                        LMotion.Create(startRot.z, startRot.z + spinAngle, flyTime)
                            .WithEase(Ease.InBack) // 加速感
                            .BindToLocalEulerAnglesZ(transform)
                    )
                    .Run()
            )
            .Run();
    }
    public void PlayDeath()
    {
        // ✅ 全部用世界坐标
        Vector3 startPos = transform.position;
        Vector3 bouncePeak = startPos + Vector3.up * bounceHeight;
        Vector3 endPos = targetPoint.position;

        Vector3 startRot = transform.localEulerAngles;

        LSequence.Create()

            // === 阶段1：弹跳（只 Y 轴） ===
            .Append(
                LMotion.Create(startPos, bouncePeak, bounceTime)
                    .WithEase(Ease.OutBounce)
                    .Bind(pos => transform.position = pos)
            )

            // === 阶段2：斜飞 + 旋转 ===
            .Append(
                LSequence.Create()
                    .Join(
                        LMotion.Create(bouncePeak, endPos, flyTime)
                            .WithEase(Ease.InOutQuad)
                            .Bind(pos => transform.position = pos)
                    )
                    .Join(
                        LMotion.Create(startRot.z, startRot.z + spinAngle, flyTime)
                            .WithEase(Ease.InBack)
                            .BindToLocalEulerAnglesZ(transform)
                    )
                    .Run()
            )

          
            .Run();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Stop();
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            PlayDeath();
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            PlayDeath1();
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            PlayDeath2();
        }
    }

    public void Test()
    {
   //     var motion2 = LMotion.Create(Vector3.zero, Vector3.one, 1f)
   //         .BindToLocalScale(transform);       // 示例：缩放动画

   //     var motion1 = LMotion.Create(transform.localEulerAngles.y, transform.localEulerAngles.y + 360f, timer)
   //.WithLoops(1,loopType) // -1 代表无限循环
   //.BindToLocalEulerAnglesZ(transform); // 专门绑定 Y 轴，无GC

        // 创建序列，按顺序播放
        LSequence.Create()
             // 先播放动画1
            //.Append(LMotion.Create(Vector3.one,Vector3.one*2, 0.2f)
            //.WithLoops(1, LoopType.Yoyo)
            //.BindToLocalScale(transform))
            //.Append(LMotion.Create(Vector3.one*2, Vector3.one, 0.5f)
            //.WithLoops(1, LoopType.Yoyo)
            //.BindToLocalScale(transform))
             .Append(LMotion.Create(transform.localEulerAngles.y, transform.localEulerAngles.y + 360f, timer)
   .WithLoops(1, loopType)
   .BindToLocalEulerAnglesZ(transform))
            .Run();               // 启动序列
    }
    public void Stop()
    {
        if (!_currentMotion.IsActive())
            return;

        if (_currentMotion.PlaybackSpeed > 0f)
            _currentMotion.PlaybackSpeed = 0f;
        else
            _currentMotion.PlaybackSpeed = 1f;
    }
}
