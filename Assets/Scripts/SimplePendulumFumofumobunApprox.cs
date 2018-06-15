//-----------------------------------------------------------------------
// <copyright file="SimplePendulumFumofumobunApprox.cs" company="dc1394's software">
//     Copyright © 2018 @dc1394 All Rights Reserved.
//     This software is released under the BSD 2-Clause License.
// </copyright>
//-----------------------------------------------------------------------
namespace SimplePendulum
{
    using System;
    using UnityEngine;

    /// <summary>
    /// 振り子クラスの実装
    /// </summary>
    internal class SimplePendulumFumofumobunApprox : MonoBehaviour
    {
        #region フィールド

        /// <summary>
        /// 重力ベクトルの向き
        /// </summary>
        private static readonly Vector3 GravityDirection = Physics.gravity.normalized;

        /// <summary>
        /// ロープの重心のスケール
        /// </summary>
        [SerializeField]
        private float centerOfGarvityForRopeScale = 0.4f;

        /// <summary>
        /// θの初期値
        /// </summary>
        private float firsttheta;

        /// <summary>
        /// 原点の座標
        /// </summary>
        [SerializeField]
        private Vector3 origin = Vector3.zero;

        /// <summary>
        /// 前回のフレームで取得した時間
        /// </summary>
        private float previousTime = 0.0f;

        /// <summary>
        /// 球の半径
        /// </summary>
        [SerializeField]
        private float radius;
                
        /// <summary>
        /// ロープオブジェクト
        /// </summary>
        [SerializeField]
        private GameObject rope = null;

        /// <summary>
        /// ロープの長さ
        /// </summary>
        private float ropeLength;

        /// <summary>
        /// 球オブジェクト
        /// </summary>
        [SerializeField]
        private GameObject sphere = null;

        #endregion フィールド

        #region メソッド

        /// <summary>
        /// 原点から測定した、球の座標と重力ベクトルの成す角度を与える関数
        /// </summary>
        /// <returns>球の座標と重力ベクトルの成す角度</returns>
        private float GetSphereAndGravityAngle()
        {
            return Vector3.Angle(this.sphere.transform.position - this.origin, SimplePendulumFumofumobunApprox.GravityDirection);
        }

        /// <summary>
        /// 原点から測定した、球の座標のz座標を与える関数
        /// </summary>
        /// <returns>球の座標のz座標</returns>
        private float GetSpherePosZ()
        {
            return (this.sphere.transform.position - this.origin).z;
        }

        /// <summary>
        /// 座標と球が成す角を計算する
        /// </summary>
        /// <returns>座標と球が成す角</returns>
        private float GetThetaDeg()
        {
            var theta = this.GetSphereAndGravityAngle();

            return theta * (this.GetSpherePosZ() > 0.0f ? 1.0f : -1.0f);
        }

        /// <summary>
        /// GUIイベントの処理
        /// </summary>
        private void OnGUI()
        {
            // GUIの見た目を変える
            var styleState = new GUIStyleState()
            {
                // テキストの色を設定
                textColor = Color.green
            };

            // スタイルの設定
            var guiStyle = new GUIStyle()
            {
                normal = styleState
            };
            
            // ラベルに近似関数による角度θの値を表示する
            GUI.Label(
                new Rect(20.0f, 60.0f, 450.0f, 20.0f),
                String.Format("@fumofumobunさんの近似関数による角度θ = {0:F3}°", this.GetThetaDeg()),
                guiStyle);

            // ラベルに近似関数による速度vの値を表示する
            GUI.Label(
                new Rect(20.0f, 100.0f, 450.0f, 20.0f),
                String.Format("@fumofumobunさんの近似関数による速度v = {0:F3}(m/s)", Solveeomcs.SolveEoMcs.GetV_Fumofumobun_Approx()),
                guiStyle);

            var kinetic = Solveeomcs.SolveEoMcs.Kinetic_Energy(Solveeomcs.SolveEoMcs.GetV_Fumofumobun_Approx());

            // ラベルに近似関数による運動エネルギーの値を表示する
            GUI.Label(
                new Rect(20.0f, 140.0f, 450.0f, 20.0f),
                String.Format("@fumofumobunさんの近似関数による運動エネルギー = {0:F3}(J)", kinetic),
                guiStyle);

            var potential = Solveeomcs.SolveEoMcs.Potential_Energy(Solveeomcs.SolveEoMcs.GetTheta_Fumofumobun_Approx());

            // ラベルに近似関数によるポテンシャルエネルギーの値を表示する
            GUI.Label(
                new Rect(20.0f, 180.0f, 450.0f, 20.0f),
                String.Format("@fumofumobunさんの近似関数によるポテンシャルエネルギー = {0:F3}(J)", potential),
                guiStyle);

            // ラベルに近似関数による全エネルギーの値を表示する
            GUI.Label(
                new Rect(20.0f, 220.0f, 450.0f, 20.0f),
                String.Format("@fumofumobunさんの近似関数による全エネルギー = {0:F3}(J)", kinetic + potential),
                guiStyle);


            // 角度θが変更された際の処理
            if (SimplePendulum.IsAngelChange)
            {
                var theta = Mathf.Deg2Rad * SimplePendulum.Thetadeg;

                Solveeomcs.SolveEoMcs.TimeReset();
                Solveeomcs.SolveEoMcs.SetTheta0(theta);
                
                this.SphereRotate(theta);
                this.RopeUpdate();
                
                SimplePendulum.IsAngelChange = false;
            }

            // 「Reset」ボタンの処理
            if (SimplePendulum.IsReset)
            {
                Solveeomcs.SolveEoMcs.SetTheta0(this.firsttheta);
                Solveeomcs.SolveEoMcs.TimeReset();

                this.SphereRotate(this.firsttheta);
                this.RopeUpdate();

                SimplePendulum.IsReset = false;
            }
        }

        /// <summary>
        /// ロープの座標と角度を更新する
        /// </summary>
        private void RopeUpdate()
        {
            // ロープの座標を更新
            this.rope.transform.position = new Vector3(
                0.0f,
                this.centerOfGarvityForRopeScale * this.sphere.transform.position.y,
                this.centerOfGarvityForRopeScale * this.sphere.transform.position.z);

            // ロープの角度を初期化
            this.rope.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

            // 角度が正かどうか
            if (this.GetSpherePosZ() > 0.0f)
            {
                this.rope.transform.Rotate(new Vector3(-this.GetSphereAndGravityAngle(), 0.0f, 0.0f));
            }
            else
            {
                this.rope.transform.Rotate(new Vector3(this.GetSphereAndGravityAngle(), 0.0f, 0.0f));
            }
        }

        /// <summary>
        /// Use this for initialization
        /// </summary>
        private void Start()
        {
            this.PendulumInit();
        }

        /// <summary>
        /// 球の角度を更新する
        /// </summary>
        /// <param name="theta">新しい角度</param>
        private void SphereRotate(float theta)
        {
            // rcosθの計算
            var rcostheta = this.ropeLength * Mathf.Cos(theta);

            // rsinθの計算
            var rsintheta = this.ropeLength * Mathf.Sin(theta);

            // 球の角度を更新
            this.sphere.transform.position = new Vector3(
                0.0f,
                (rsintheta * SimplePendulumFumofumobunApprox.GravityDirection.z) + (rcostheta * SimplePendulumFumofumobunApprox.GravityDirection.y),
                (rcostheta * SimplePendulumFumofumobunApprox.GravityDirection.z) - (rsintheta * SimplePendulumFumofumobunApprox.GravityDirection.y));
        }

        /// <summary>
        /// フレーム処理
        /// </summary>
        private void Update()
        {
            // 時間差を取得
            var frameTime = Time.time - this.previousTime;

            // 新しく取得した時間を代入
            this.previousTime = Time.time;

            if (SimplePendulum.Exec)
            {
                // 球の座標を更新
                this.SphereUpdate(frameTime);

                // ロープの座標と角度を更新
                this.RopeUpdate();
            }
        }

        /// <summary>
        /// 振り子の状態を初期化する
        /// </summary>
        private void PendulumInit()
        {
            this.ropeLength = Vector3.Distance(this.origin, this.sphere.transform.position);

            this.firsttheta = Mathf.Deg2Rad * this.GetThetaDeg();
        }

        /// <summary>
        /// 球の状態を更新する
        /// </summary>
        /// <param name="frameTime">経過時間</param>
        private void SphereUpdate(float frameTime)
        {
            // 経過時間を更新
            Solveeomcs.SolveEoMcs.SetTime(frameTime);

            // @fumofumobunさんの近似関数から角度θを求める
            var theta = Solveeomcs.SolveEoMcs.GetTheta_Fumofumobun_Approx();

            // 球の角度を更新
            this.SphereRotate(theta);
        }

        #endregion メソッド
    }
}
