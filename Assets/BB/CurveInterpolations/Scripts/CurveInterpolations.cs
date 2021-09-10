using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if USE_DOUBLE
using Real = System.Double;
#else
using Real = System.Single;
#endif

#if NOTUNITY
using Vect = System.Numerics.Vector3;
#else
using UVector = UnityEngine.Vector3;
#endif
namespace CurveLib.Interpolation {
	public class CurveInterpolations
	{
		public static UVector CatmullRom(Real t, UVector p0, UVector p1, UVector p2, UVector p3 )
		{

			UVector v0 = (p2 - p0) * (Real)0.5;
			UVector v1 = (p3 - p1) * (Real)0.5;
			Real t2 = t * t;
			Real t3 = t * t2;
			return (2 * p1 - 2 * p2 + v0 + v1) * t3 + (-3 * p1 + 3 * p2 - 2 * v0 - v1) * t2 + v0 * t + p1;

		}

		//

		private static UVector QuadraticBezierP0(Real t, UVector p )
		{

			var k = 1 - t;
			return k * k * p;

		}

		private static UVector QuadraticBezierP1(Real t, UVector p )
		{

			return 2 * (1 - t) * t * p;

		}

		private static UVector QuadraticBezierP2(Real t, UVector p )
		{

			return t * t * p;

		}

		public static UVector QuadraticBezier(Real t, UVector p0, UVector p1, UVector p2)
		{

			return QuadraticBezierP0(t, p0) + QuadraticBezierP1(t, p1) +
				QuadraticBezierP2(t, p2);
		}

		//

		private static UVector CubicBezierP0(Real t, UVector p )
		{

			var k = 1 - t;
			return k * k * k * p;

		}

		private static UVector CubicBezierP1(Real t, UVector p )
		{

			var k = 1 - t;
			return 3 * k * k * t * p;

		}

		private static UVector CubicBezierP2(Real t, UVector p )
		{

			return 3 * (1 - t) * t * t * p;

		}

		private static UVector CubicBezierP3(Real t, UVector p )
		{

			return t * t * t * p;

		}

		public static UVector CubicBezier(Real t, UVector p0, UVector p1, UVector p2, UVector p3 )
		{
			return CubicBezierP0(t, p0) + CubicBezierP1(t, p1) + CubicBezierP2(t, p2) +
				CubicBezierP3(t, p3);
		}

	}
}