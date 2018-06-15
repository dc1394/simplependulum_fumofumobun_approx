/*! \file solveeommain.h
    \brief 単振り子に対して運動方程式を解く関数群の宣言

    Copyright © 2016-2018 @dc1394 All Rights Reserved.
    This software is released under the BSD 2-Clause License.
*/
#ifndef _SOLVEEOMMAIN_H_
#define _SOLVEEOMMAIN_H_

#ifdef __cplusplus
#define DLLEXPORT extern "C" __declspec(dllexport)
#else
#define DLLEXPORT __declspec(dllexport)
#endif

#include "solveeom.h"
#include <optional>		// for std::optional

extern "C" {
    //! A global variable.
    /*!
        SolveEOMクラスのオブジェクトへのポインタ
    */
    static std::optional<solveeom::SolveEoM> pse;
    
    //! A global function.
    /*!
        角度θの値に対するgetter
        \return 角度θ
    */
    DLLEXPORT float __stdcall gettheta();
    
	//! A global function.
	/*!
		@fumofumobunさんの近似関数によって、角度θを求める
		\return @fumofumobunさんの近似関数によって求めた角度θ
	*/
	DLLEXPORT float __stdcall gettheta_fumofumobun_approx();

	//! A global function.
	/*!
		@fumofumobunさんの近似関数によって、速度vを求める
		\return @fumofumobunさんの近似関数によって求めた速度v
	*/
	DLLEXPORT float __stdcall getv_fumofumobun_approx();

    //! A global function.
    /*!
        速度vの値に対するgetter
        \return 速度v
    */
    DLLEXPORT float __stdcall getv();

    //! A global function.
    /*!
        seオブジェクトを初期化する
        \param l ロープの長さ
        \param r 球の半径
        \param theta0 θの初期値
    */
    DLLEXPORT void __stdcall init(float l, float r, float theta0);

    //! A global function.
    /*!
        次のステップを計算する
        \param dt 前ステップからの経過時間
        \return 新しい角度θの値
    */
    DLLEXPORT float __stdcall nextstep(float dt);

    //! A global function.
    /*!
        運動方程式を、指定された時間まで積分し、その結果を時間間隔Δtごとにファイルに保存する
        \param dt 時間刻み
        \param filename 保存ファイル名
        \param t 指定時間
    */
    DLLEXPORT void __stdcall saveresult(double dt, std::string const & filename, double t);

	//! A global function.
    /*!
        角度θの値に対するsetter
        \param theta 設定する角度θ
    */
    DLLEXPORT void __stdcall settheta(float theta);

	//! A global function.
	/*!
		初期角度θ₀の値に対するsetter
		\param theta0 設定する初期角度θ₀
	*/
	DLLEXPORT void __stdcall settheta0(float theta0);

	//! A global function.
	/*!
		経過時間tに対するsetter
		\param dt 前ステップからの経過時間
	*/
	DLLEXPORT void __stdcall settime(float dt);
	
	//! A global function.
	/*!
		速度vの値に対するsetter
		\param v 設定する速度v
	*/
	DLLEXPORT void __stdcall setv(float v);

	//! A global function.
	/*!
		経過時間tを初期値（= 0.0）に戻す
	*/
	DLLEXPORT void __stdcall timereset();

	//! A global function.
	/*!
		全エネルギーを求める
		\return 全エネルギー
	*/
	DLLEXPORT float __stdcall total_energy();

	//! A global function.
	/*!
		@fumofumbunさんの近似関数によって、全エネルギーを求める
		\return @fumofumbunさんの近似関数によって求めた全エネルギー
	*/
	DLLEXPORT float __stdcall total_energy_fumofumobun_approx();
}

#endif  // _SOLVEEOMMAIN_H_

