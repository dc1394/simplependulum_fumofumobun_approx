//-----------------------------------------------------------------------
// <copyright file="SolveEoMcs.cs" company="dc1394's software">
//     Copyright © 2016-2018 @dc1394 All Rights Reserved.
//     This software is released under the BSD 2-Clause License.
// </copyright>
//-----------------------------------------------------------------------
namespace Solveeomcs
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// C++で書かれたSolveEOMクラスをC#からアクセスするためのラッパークラス
    /// </summary>
    public sealed class SolveEoMcs
    {
        #region メソッド

        /// <summary>
        /// 角度θの値に対するgetter
        /// </summary>
        /// <returns>角度θ</returns>
        [DllImport("solveeom", EntryPoint = "gettheta")]
        public static extern Single GetTheta();

        /// <summary>
        /// @fumofumobunさんの近似関数によって、角度θを求める
        /// </summary>
        /// <returns>@fumofumobunさんの近似関数によって求めた角度θ</returns>
        [DllImport("solveeom", EntryPoint = "gettheta_fumofumobun_approx")]
        public static extern Single GetTheta_Fumofumobun_Approx();
        
        /// <summary>
        /// 速度vの値に対するgetter
        /// </summary>
        /// <returns>速度v</returns>
        [DllImport("solveeom", EntryPoint = "getv")]
        public static extern Single GetV();

        /// <summary>
        /// @fumofumobunさんの近似関数によって、速度vを求める
        /// </summary>
        /// <returns>@fumofumobunさんの近似関数によって求めた速度v</returns>
        [DllImport("solveeom", EntryPoint = "getv_fumofumobun_approx")]
        public static extern Single GetV_Fumofumobun_Approx();

        /// <summary>
        /// seオブジェクトを初期化する
        /// </summary>
        /// <param name="l">ロープの長さ</param>
        /// <param name="r">球の半径</param>
        /// <param name="theta0">角度θの初期値θ₀</param>
        [DllImport("solveeom", EntryPoint = "init")]
        public static extern void Init(Single l, Single r, Single theta0);

        /// <summary>
        /// 次のステップを計算する
        /// </summary>
        /// <param name="dt">前ステップからの経過時間</param>
        /// <returns>新しい角度θの値</returns>
        [DllImport("solveeom", EntryPoint = "nextstep")]
        public static extern Single NextStep(Single dt);

        /// <summary>
        /// 角度θの値に対するsetter
        /// </summary>
        /// <param name="theta">設定する角度θ</param>
        [DllImport("solveeom", EntryPoint = "settheta")]
        public static extern void SetTheta(Single theta);

        /// <summary>
        /// 初期角度θ₀の値に対するsetter
        /// </summary>
        /// <param name="theta0">設定する初期角度θ</param>
        [DllImport("solveeom", EntryPoint = "settheta0")]
        public static extern void SetTheta0(Single theta0);

        /// <summary>
        /// 経過時間tに対するsetter
        /// </summary>
        /// <param name="dt">前ステップからの経過時間</param>
        [DllImport("solveeom", EntryPoint = "settime")]
        public static extern void SetTime(Single dt);

        /// <summary>
        /// 速度vの値に対するsetter
        /// </summary>
        /// <param name="v">設定する速度v</param>
        [DllImport("solveeom", EntryPoint = "setv")]
        public static extern void SetV(Single v);

        /// <summary>
        /// 経過時間tを初期値（= 0.0）に戻す
        /// </summary>
        [DllImport("solveeom", EntryPoint = "timereset")]
        public static extern void TimeReset();

        /// <summary>
        /// 全エネルギーを求める
        /// </summary>
        /// <returns>全エネルギー</returns>
        [DllImport("solveeom", EntryPoint = "total_energy")]
        public static extern Single Total_Energy();

        /// <summary>
        /// @fumofumbunさんの近似関数によって、全エネルギーを求める
        /// </summary>
        /// <returns>@fumofumbunさんの近似関数によって求めた全エネルギー</returns>
        [DllImport("solveeom", EntryPoint = "total_energy_fumofumobun_approx")]
        public static extern Single Total_Energy_Fumofumobun_Approx();

        #endregion メソッド
    }
}