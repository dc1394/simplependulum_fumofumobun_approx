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
		Isconsider_Inertial_Resistance(nullptr, [this](auto isconsider_inertial_resistance) { return isconsider_inertial_resistance_ = isconsider_inertial_resistance; }),
        Theta([this] { return static_cast<float>(x_[0]); }, [this](auto theta) { return x_[0] = theta; }),
		Theta0(nullptr, [this](auto theta0) { return theta0_ = theta0; }),
		Time([this] { return static_cast<float>(t_); }, [this](auto t) { return t_ = t; }),
		V([this] { return static_cast<float>(l_ * x_[1]); }, [this](auto v) { return x_[1] = v / l_; }),
        l_(l),
		omega0_2_(g / l_),
        r_(r),
		m_(4.0 / 3.0 * boost::math::constants::pi<double>() * r * r * r * SolveEoM::ALUMINIUMRHO),
		gamma_(3.0 * boost::math::constants::pi<double>() * r_ * AIRMYU / m_),
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
		return static_cast<float>(theta0_ * std::exp(-gamma_ * t_) *
								  std::cos(std::sqrt((omega0_2_ - gamma_ * gamma_) * (3.0 + std::cos(theta0_ * std::exp(-gamma_ * t_)))) / 2.0 * t_));
	}

	float SolveEoM::getv_fumofumobun_approx() const
    {
		auto const term1 = -gamma_ * gettheta_fumofumobun_approx();

		auto const alpha = 0.5 * std::sqrt((omega0_2_ - gamma_ * gamma_) * (3.0 + std::cos(theta0_ * std::exp(-gamma_ * t_))));

		auto const term2 = -alpha * theta0_* std::exp(-gamma_ * t_) * std::sin(alpha * t_) *
						   (0.5 * theta0_ * gamma_ * std::exp(-gamma_ * t_) * std::sin(theta0_ * std::exp(-gamma_ * t_)) /
						   (3.0 + std::cos(theta0_ * std::exp(-gamma_ * t_))) * t_ + 1.0);

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
            [&result, this](auto const & x, auto const t)
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

			// レイノルズ数
			auto const Re = 2.0 * r_ * std::fabs(l_ * x[1]) / AIRNYU;

            // 粘性抵抗
            auto const F = 6.0 * boost::math::constants::pi<double>() * AIRMYU * r_ * l_ * x[1];

			// レイノルズ数が閾値より小さいか、「慣性抵抗も考慮」チェックボックスが外れていたら
			if (Re < SolveEoM::REYNOLDS_THRESHOLD || !isconsider_inertial_resistance_) {
				// 粘性抵抗のみを考慮する
				dxdt[1] = f1 - F / (m_ * l_);

				return;
			}

			auto const FD = 0.5 * AIRRHO * boost::math::constants::pi<double>() * sqr(r_ * (l_ * x[1]));

			// Drag coefficient
			double CD;

			// N.-S. Cheng, Comparison of formulas for drag coefficient and settling velocity of
			// spherical particles, Powder Technology 189 (2009) 395–398.
			if (Re <= 3000) {
				CD = 24.0 / Re * std::pow(1.0 + 0.27 * Re, 0.43) + 0.47 * (1.0 - std::exp(-0.04 * std::pow(Re, 0.38)));
			}
			else {
				// Re > 3000
				// Almedeij J. Drag coefficient of flow around a sphere: Matching asymptotically the wide
				// trend. std::powder Technology. (2008);doi:10.1016/j.std::powtec.2007.12.006.
				auto const phi1 = std::pow(24.0 / Re, 10) + std::pow(21.0 * std::pow(Re, -0.67), 10) +
					std::pow(4.0 * std::pow(Re, -0.33), 10) + std::pow(0.4, 10);
				auto const phi2 = 1.0 / (1.0 / std::pow(0.148 * std::pow(Re, 0.11), 10) + 1.0 / std::pow(0.5, 10));
				auto const phi3 = std::pow((1.57E+8) * std::pow(Re, -1.625), 10);
				auto const phi4 = 1.0 / (1.0 / std::pow((6.0E-17) * std::pow(Re, 2.63), 10) + 1.0 / std::pow(0.2, 10));

				CD = std::pow((1.0 / (1.0 / (phi1 + phi2) + 1.0 / phi3) + phi4), 0.1);
			}

			// 慣性抵抗÷(m×l)
			auto const f2 = (x[1] >= 0.0) ? -FD * CD / (m_ * l_) : FD * CD / (m_ * l_);
			dxdt[1] = f1 + f2 - F / (m_ * l_);
        };

        return eom;
    }

    // #endregion privateメンバ関数
}
