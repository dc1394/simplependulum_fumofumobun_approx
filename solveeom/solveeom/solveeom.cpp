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

namespace solveeom {
    // #region コンストラクタ・デストラクタ

    SolveEoM::SolveEoM(float l, float r, bool resistance, bool simpleharmonic, float theta0) :
        Resistance(nullptr, [this](auto resistance) { return resistance_ = resistance; }),
		Simpleharmonic(nullptr, [this](auto simpleharmonic) { return simpleharmonic_ = simpleharmonic; }),
        Theta([this] { return static_cast<float>(x_[0]); }, [this](auto theta) { return x_[0] = theta; }),
		V([this] { return static_cast<float>(x_[1]); }, [this](auto v) { return x_[1] = v; }),
        l_(l),
        r_(r),
        m_(4.0 / 3.0 * boost::math::constants::pi<double>() * r * r * r * SolveEoM::ALUMINIUMRHO),
        myu_(SolveEoM::AIRMYU),     // 空気の粘度
        resistance_(resistance),    // 空気抵抗
        rho_(SolveEoM::AIRRHO),     // 空気の密度
		simpleharmonic_(simpleharmonic),
        stepper_(SolveEoM::EPS, SolveEoM::EPS),
		t_(0.0),
		theta0_(theta0),
		x_({ theta0, 0.0 })
    {
        nyu_ = myu_ / rho_;
    }

    // #endregion コンストラクタ・デストラクタ

    // #region publicメンバ関数

	float SolveEoM::gettheta_fumofumobun_approx(float dt)
	{
		static auto const gamma = static_cast<float>(3.0 * boost::math::constants::pi<double>() * r_ * myu_ / m_);
		static auto const omega0_2 = static_cast<float>(g / l_);

		t_ += dt;

		return static_cast<float>(theta0_ * std::exp(-gamma * t_) * std::cos(std::sqrt((omega0_2 - gamma * gamma) * (3.0 + std::cos(theta0_ * std::exp(-gamma * t_)))) / 2.0 * t_));
	}

    float SolveEoM::kinetic_energy() const
    {
        return static_cast<float>(0.5 * m_ * sqr(l_ * x_[1]));
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
				result << boost::format("%.3f, %.15f, %.15f\n") % t % x[0] % gettheta_fumofumobun_approx(dt);
            });
    }

    float SolveEoM::potential_energy() const
    {
        return static_cast<float>(m_ * SolveEoM::g * l_ * (1.0f - std::cos(x_[0])));
    }
	
    void SolveEoM::setfluid(std::int32_t fluid)
    {
        switch (static_cast<SolveEoM::Fluid_type>(fluid)) {
        case SolveEoM::Fluid_type::AIR:
            myu_ = SolveEoM::AIRMYU;
            rho_ = SolveEoM::AIRRHO;
            break;

        case SolveEoM::Fluid_type::WATER:
            myu_ = SolveEoM::WATERMYU;
            rho_ = SolveEoM::WATERRHO;
            break;

        default:
            BOOST_ASSERT(!"ここに来てはいけない!");
            break;
        }

        nyu_ = myu_ / rho_;
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
	
	double SolveEoM::total_energy() const
	{
		auto const kinetic = 0.5 * m_ * sqr(l_ * x_[1]);
		auto const potential = m_ * SolveEoM::g * l_ * (1.0 - std::cos(x_[0]));

		return kinetic + potential;
	}

    // #endregion privateメンバ関数
}