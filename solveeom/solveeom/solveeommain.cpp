/*! \file solveeommain.cpp
    \brief 単振り子に対して運動方程式を解く関数群の実装

    Copyright © 2016-2018 @dc1394 All Rights Reserved.
    This software is released under the BSD 2-Clause License.
*/
#include "solveeommain.h"

extern "C" {
    float __stdcall gettheta()
    {
        return pse->Theta();
    }

	float __stdcall gettheta_fumofumobun_approx()
	{
		return pse->gettheta_fumofumobun_approx();
	}

	float __stdcall getv_fumofumobun_approx()
	{
		return pse->getv_fumofumobun_approx();
	}

    float __stdcall getv()
    {
        return pse->V();
    }

    void __stdcall init(float l, float r, float theta0)
    {
        pse.emplace(l, r, theta0);
    }
    
    float __stdcall nextstep(float dt)
    {
        return (*pse)(dt);
    }

    void __stdcall saveresult(double dt, std::string const & filename, double t)
    {
        (*pse)(dt, filename, t);
    }
	    
    void __stdcall settheta(float theta)
    {
        pse->Theta = theta;
    }

	void __stdcall settheta0(float theta0)
	{
		pse->Theta0 = theta0;
	}

	void __stdcall settime(float dt)
    {
		pse->Time = pse->Time() + dt;
    }

    void __stdcall setv(float v)
    {
        pse->V(v);
    }

	void __stdcall timereset()
	{
		pse->timereset();
	}

	float __stdcall total_energy()
	{
		return pse->total_energy();
	}

	float __stdcall total_energy_fumofumobun_approx()
    {
		return pse->total_energy_fumofumobun_approx();
    }
}
