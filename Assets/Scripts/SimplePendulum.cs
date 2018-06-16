//-----------------------------------------------------------------------
// <copyright file="SimplePendulum.cs" company="dc1394's software">
//     Copyright ©  2016 @dc1394 All Rights Reserved.
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
    internal class SimplePendulum : MonoBehaviour
    {
        #region フィールド

        /// <summary>
        /// ボタンの「Start」テキスト
        /// </summary>
        private const String StartText = "Start";

        /// <summary>
        /// ボタンの「Stop」テキスト
        /// </summary>
        private const String StopText = "Stop";

        /// <summary>
        /// 重力ベクトルの向き
        /// </summary>
        private static readonly Vector3 GravityDirection = Physics.gravity.normalized;

        /// <summary>
        /// 角度θ(deg)
        /// </summary>
        private static Single thetadeg;

        /// <summary>
        /// ボタンのテキスト
        /// </summary>
        private String buttontext = SimplePendulum.StartText;

        /// <summary>
        /// ロープの重心のスケール
        /// </summary>
        [SerializeField]
        private Single centerOfGarvityForRopeScale = 0.4f;
                
        /// <summary>
        /// θの初期値
        /// </summary>
        private Single firsttheta;

        /// <summary>
        /// 慣性抵抗を考慮するかどうか
        /// </summary>
        private Boolean isconsiderInertialResistance = false;

        /// <summary>
        /// 原点の座標
        /// </summary>
        [SerializeField]
        private Vector3 origin = Vector3.zero;

        /// <summary>
        /// 前回のフレームで取得した時間
        /// </summary>
        private Single previousTime = 0.0f;

        /// <summary>
        /// 球の半径
        /// </summary>
        [SerializeField]
        private Single radius = 0.05f;

        /// <summary>
        /// ロープオブジェクト
        /// </summary>
        [SerializeField]
        private GameObject rope = null;

        /// <summary>
        /// ロープの長さ
        /// </summary>
        private Single ropeLength;

        /// <summary>
        /// 球オブジェクト
        /// </summary>
        [SerializeField]
        private GameObject sphere = null;

        /// <summary>
        /// 経過時間t(sec)
        /// </summary>
        private Single time;

        #endregion フィールド

        #region プロパティ

        /// <summary>
        /// 実行中かどうかを示すフラグへのプロパティ
        /// </summary>
        internal static Boolean Exec { get; set; }
        
        /// <summary>
        /// 「Reset」ボタンが押されたかどうかへのプロパティ
        /// </summary>
        internal static Boolean IsChangeOfState { get; set; }

        /// <summary>
        /// 角度θ(deg)へのプロパティ
        /// </summary>
        internal static Single Thetadeg
        {
            get
            {
                return SimplePendulum.thetadeg;
            }
        }

        #endregion プロパティ

        #region メソッド

        /// <summary>
        /// 原点から測定した、球の座標と重力ベクトルの成す角度を与える関数
        /// </summary>
        /// <returns>球の座標と重力ベクトルの成す角度</returns>
        private Single GetSphereAndGravityAngle()
        {
            return Vector3.Angle(this.sphere.transform.position - this.origin, SimplePendulum.GravityDirection);
        }

        /// <summary>
        /// 原点から測定した、球の座標のz座標を与える関数
        /// </summary>
        /// <returns>球の座標のz座標</returns>
        private Single GetSpherePosZ()
        {
            return (this.sphere.transform.position - this.origin).z;
        }

        /// <summary>
        /// 座標と球が成す角を計算する
        /// </summary>
        /// <returns>座標と球が成す角</returns>
        private Single GetThetaDeg()
        {
            var theta = this.GetSphereAndGravityAngle();

            return theta * (this.GetSpherePosZ() > 0.0f ? 1.0f : -1.0f);
        }

        /// <summary>
        /// GUIイベントの処理
        /// </summary>
        private void OnGUI()
        {
            // ラベルに経過時間tの値を表示する
            GUI.Label(
                new Rect(20.0f, 20.0f, 450.0f, 20.0f),
                String.Format("経過時間{0:F3}（秒）", this.time));

            // ラベルに数値的に求めた角度θの値を表示する
            GUI.Label(
                new Rect(20.0f, 40.0f, 450.0f, 20.0f),
                String.Format("数値的に求めた角度θ =                                 {0:F3}°", this.GetThetaDeg()));

            // ラベルに数値的に求めた速度vの値を表示する
            GUI.Label(
                new Rect(20.0f, 80.0f, 450.0f, 20.0f),
                String.Format("数値的に求めた速度v =                                  {0:F3}(m/s)", Solveeomcs.SolveEoMcs.GetV()));

            var kinetic = Solveeomcs.SolveEoMcs.Kinetic_Energy(Solveeomcs.SolveEoMcs.GetV());

            // ラベルに数値的に求めた運動エネルギーの値を表示する
            GUI.Label(
                new Rect(20.0f, 120.0f, 450.0f, 20.0f),
                String.Format("数値的に求めた運動エネルギー =                                 {0:F3}(J)", kinetic));

            var potential = Solveeomcs.SolveEoMcs.Potential_Energy(Solveeomcs.SolveEoMcs.GetTheta());

            // ラベルに数値的に求めたポテンシャルエネルギーの値を表示する
            GUI.Label(
                new Rect(20.0f, 160.0f, 450.0f, 20.0f),
                String.Format("数値的に求めたポテンシャルエネルギー =                                 {0:F3}(J)", potential));

            // ラベルに数値的に求めたポテンシャルエネルギーの値を表示する
            GUI.Label(
                new Rect(20.0f, 200.0f, 450.0f, 20.0f),
                String.Format("数値的に求めた全エネルギー =                                 {0:F3}(J)", kinetic + potential));

            var ypos2 = 20.0f;

            // 「角度θ」と表示する
            GUI.Label(new Rect(470.0f, ypos2, 100.0f, 20.0f), "角度θ");

            ypos2 += 20.0f;

            // 角度を変更するスライダーを表示する
            var thetadegbefore = SimplePendulum.thetadeg;
            SimplePendulum.thetadeg = GUI.HorizontalSlider(new Rect(470.0f, ypos2, 100.0f, 20.0f), SimplePendulum.thetadeg, -180.0f, 180.0f);
            if (Mathf.Abs(SimplePendulum.thetadeg - thetadegbefore) > Mathf.Epsilon)
            {
                this.Reset(Mathf.Deg2Rad * SimplePendulum.thetadeg);
            }

            ypos2 += 20.0f;

            // 慣性抵抗を考慮するかどうかのチェックボックスを表示する
            var isconsiderInertialResistanceBefore = this.isconsiderInertialResistance;
            this.isconsiderInertialResistance = GUI.Toggle(new Rect(470.0f, ypos2, 110.0f, 20.0f), this.isconsiderInertialResistance, "慣性抵抗を考慮");
            if (isconsiderInertialResistanceBefore != this.isconsiderInertialResistance)
            {
                Solveeomcs.SolveEoMcs.SetIsconsider_Inertial_Resistance(this.isconsiderInertialResistance);
                this.Reset(this.firsttheta);
            }

            var ypos3 = 20.0f;

            // 「Start」か「Stop」ボタンを表示する
            if (GUI.Button(new Rect(600.0f, ypos3, 110.0f, 20.0f), this.buttontext))
            {
                if (SimplePendulum.Exec)
                {
                    SimplePendulum.Exec = false;
                    this.buttontext = SimplePendulum.StartText;
                }
                else
                {
                    SimplePendulum.Exec = true;
                    this.buttontext = SimplePendulum.StopText;
                }
            }

            ypos3 += 30.0f;

            // 「Reset」ボタンを表示する
            if (GUI.Button(new Rect(600.0f, ypos3, 110.0f, 20.0f), "Reset"))
            {
                this.Reset(this.firsttheta);
            }

            ypos3 += 30.0f;

            // 「Exit」ボタンを表示する
            if (GUI.Button(new Rect(600.0f, ypos3, 110.0f, 20.0f), "Exit"))
            {
                Application.Quit();
            }
        }

        /// <summary>
        /// 角度θを指定して、時刻0の状態に戻す
        /// </summary>
        /// <param name="theta">角度θ</param>
        private void Reset(float theta)
        {
            SimplePendulum.IsChangeOfState = true;

            this.time = 0.0f;

            SimplePendulum.thetadeg = theta * Mathf.Rad2Deg;
            Solveeomcs.SolveEoMcs.SetTheta(theta);
            Solveeomcs.SolveEoMcs.SetV(0.0f);

            this.SphereRotate(theta);
            this.RopeUpdate();
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
            SimplePendulum.Exec = false;
            SimplePendulum.IsChangeOfState = false;

            this.enabled = true;
            this.PendulumInit();
        }

        /// <summary>
        /// 球の角度を更新する
        /// </summary>
        /// <param name="theta">新しい角度</param>
        private void SphereRotate(Single theta)
        {
            // rcosθの計算
            var rcostheta = this.ropeLength * Mathf.Cos(theta);

            // rsinθの計算
            var rsintheta = this.ropeLength * Mathf.Sin(theta);

            // 球の角度を更新
            this.sphere.transform.position = new Vector3(
                0.0f,
                (rsintheta * SimplePendulum.GravityDirection.z) + (rcostheta * SimplePendulum.GravityDirection.y),
                (rcostheta * SimplePendulum.GravityDirection.z) - (rsintheta * SimplePendulum.GravityDirection.y));
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
                this.time += frameTime;

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

            SimplePendulum.thetadeg = this.GetThetaDeg();

            Solveeomcs.SolveEoMcs.Init(
                this.ropeLength,
                this.radius,
                this.firsttheta = Mathf.Deg2Rad * SimplePendulum.thetadeg);
        }

        /// <summary>
        /// 球の状態を更新する
        /// </summary>
        /// <param name="frameTime">経過時間</param>
        private void SphereUpdate(Single frameTime)
        {
            // 運動方程式を解いてθを求める
            var theta = Solveeomcs.SolveEoMcs.NextStep(frameTime);

            // 球の角度を更新
            this.SphereRotate(theta);
        }

        #endregion メソッド
    }
}
