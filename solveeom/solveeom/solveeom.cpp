/*! \file solveeom.cpp
    \brief 単振り子に対して運動方程式を解くクラスの実装

    Copyright © 2016-2018 @dc1394 All Rights Reserved.
    This software is released under the BSD 2-Clause License.
*/
#include "solveeom.h"
#include <cmath>                                // for std::sin, std::cos
#include <fstream>                              // for std::ofstream
#include <boost/assert.hpp>                     // for BOOST_ASSERT
#include <boost/format.hpp>                     // for boost::format
#include <boost/math/constants/constants.hpp>   // for boost::math::constants::pi
#include "solveeommain.h"

namespace solveeom {
    // #region コンストラクタ・デストラクタ

    SolveEoM::SolveEoM(float l, float r, float theta0) :
        Theta([this] { return static_cast<float>(x_[0]); }, [this](auto theta) { return x_[0] = theta; }),
		Theta0(nullptr, [this](auto theta0) { return theta0_ = theta0; }),
		Time([this] { return static_cast<float>(t_); }, [this](auto t) { return t_ = t; }),
		V([this] { return static_cast<float>(l_ * x_[1]); }, [this](auto v) { return x_[1] = v / l_; }),
        l_(l),
		omega0_2_(g / l_),
        r_(r),
        m_(4.0 / 3.0 * boost::math::constants::pi<double>() * r * r * r * SolveEoM::ALUMINIUMRHO),
        myu_(SolveEoM::AIRMYU),     // 空気の粘度
		gamma_(3.0 * boost::math::constants::pi<double>() * r_ * myu_ / m_),
		stepper_(SolveEoM::EPS, SolveEoM::EPS),
		t_(0.0),
		theta0_(theta0),
		x_({ theta0, 0.0 })
    {
    }

    // #endregion コンストラクタ・デストラクタ

    // #region publicメンバ関数

	float SolveEoM::gettheta_fumofumobun_approx() const
	{
		return static_cast<float>(theta0_ * std::exp(-gamma_ * t_) * std::cos(std::sqrt((omega0_2_ - gamma_ * gamma_) * (3.0 + std::cos(theta0_ * std::exp(-gamma_ * t_)))) / 2.0 * t_));
	}

	float SolveEoM::getv_fumofumobun_approx() const
    {
		auto const term1 = -gamma_ * gettheta_fumofumobun_approx();

		auto const alpha = 0.5 * std::sqrt((omega0_2_ - gamma_ * gamma_) * (3.0 + std::cos(theta0_ * std::exp(-gamma_ * t_))));

		auto const term2 = -alpha * theta0_* std::exp(-gamma_ * t_) * std::sin(alpha * t_) * (0.5 * theta0_ * gamma_ * std::exp(-gamma_ * t_) * std::sin(theta0_ * std::exp(-gamma_ * t_)) / (3.0 + std::cos(theta0_ * std::exp(-gamma_ * t_))) * t_ + 1.0);

		return l_ * (term1 + term2);
    }

	float SolveEoM::kinetic_energy(double v) const
	{
		return static_cast<float>(0.5 * m_ * sqr(v));
	}

    float SolveEoM::operator()(float dt)
    {
        boost::numeric::odeint::integrate_adaptive(
            stepper_,
            getEoM(),
            x_,
            0.0,
            static_cast<double>(dt),
            SolveEoM::DX);

        return static_cast<float>(x_[0]);
    }
	
    void SolveEoM::operator()(double dt, std::string const & filename, double t)
    {
        std::ofstream result(filename);

        boost::numeric::odeint::integrate_const(
            stepper_,
            getEoM(),
            x_,
            0.0,
            t,
            dt,
            [dt, &result, this](auto const & x, auto const t)
            {
				result << boost::format("%.3f, %.15f, %.15f\n") % t % x[0] % gettheta_fumofumobun_approx();
            });
    }

	float SolveEoM::potential_energy(double theta) const
	{
		return static_cast<float>(m_ * SolveEoM::g * l_ * (1.0 - std::cos(theta)));
	}

	void SolveEoM::timereset()
    {
		t_ = 0.0;
    }

    // #endregion publicメンバ関数

    // #region privateメンバ関数

	std::function<void(SolveEoM::state_type const &, SolveEoM::state_type &, double const)> SolveEoM::getEoM() const
    {
        auto const eom = [this](state_type const & x, state_type & dxdt, double const)
        {
            // dθ/dt = v / l
            dxdt[0] = x[1];

            // 振り子に働く力
            auto const f1 = -SolveEoM::g * std::sin(x[0]) / l_;

            // 粘性抵抗
            auto const F = 6.0 * boost::math::constants::pi<double>() * myu_ * r_ * l_ * x[1];

            // 粘性抵抗のみ
            dxdt[1] = f1 - F / (m_ * l_);
        };

        return eom;
    }

    // #endregion privateメンバ関数
}
