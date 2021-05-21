/*
  Copyright 2021 EPAM Systems, Inc

  See the NOTICE file distributed with this work for additional information
  regarding copyright ownership. Licensed under the Apache License,
  Version 2.0 (the "License"); you may not use this file except in compliance
  with the License.  You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
  WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the
  License for the specific language governing permissions and limitations under
  the License.
 */

using System;
using System.Runtime.CompilerServices;

namespace EPAM.Deltix.HdTime
{
	internal static class Util
	{
		// NOTE: Optimal case for exception handling still seems to be throwing from separate function, but to not try to control inlining`


		//[MethodImpl(MethodImplOptions.AggressiveInlining)] (.NET >=4.5 only)
		internal static bool InRangeBelow(int x, int upperBoundExclusive)
		{
			return (uint)x < (uint)upperBoundExclusive;
		}

		internal static bool InRangeBelow(Int64 x, UInt64 upperBoundExclusive)
		{
			return (UInt64)x < upperBoundExclusive;
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool InRange(int x, int lowerBound, int upperBound)
		{
			return (uint)unchecked(x + -lowerBound) < (uint)unchecked(upperBound - lowerBound + 1);
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool InRange(Int64 x, Int64 lowerBound, Int64 upperBound)
		{
			// The simpler code is verified to be slower on .NET 4.7/x64
			//return x >= lowerBound && x <= upperBound; 
			return (UInt64)unchecked(x + -lowerBound) < (UInt64)unchecked(upperBound - lowerBound + 1);
		}

		internal static void ThrowOutOfRange() { throw new ArgumentOutOfRangeException(); }

		//[MethodImpl(MethodImplOptions.NoInlining)]
		private static void AdditionOverflow()
		{
			throw new OverflowException("Signed Time addition overflow");
		}

		//[MethodImpl(MethodImplOptions.NoInlining)]
		private static void SubtractionOverflow()
		{
			throw new OverflowException("Signed Time subtraction overflow");
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static Int64 SubtractToDt(Int64 a, Int64 b)
		{
			Int64 x = unchecked(a - b);
			if (((a ^ b) & (a ^ x)) < 0 | !Convert.DateTime.IsValidNanos(x))
				SubtractionOverflow();

			return x;
		}

		internal static Int64 SubtractToTs(Int64 a, Int64 b)
		{
			Int64 x = unchecked(a - b);
			if (((a ^ b) & (a ^ x) | (x & -x)) < 0)
				SubtractionOverflow();

			return x;
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static Int64 Add(Int64 a, Int64 b)
		{
			Int64 x = unchecked(a + b);
			if (((a ^ ~b) & (a ^ x)) < 0)
				AdditionOverflow();
			return x;
		}

		internal static Int64 AddToDt(Int64 a, Int64 b)
		{
			Int64 x = unchecked(a + b);
			if (((a ^ ~b) & (a ^ x)) < 0 | !Convert.DateTime.IsValidNanos(x))
				AdditionOverflow();

			return x;
		}

		internal static Int64 AddToTs(Int64 a, Int64 b)
		{
			Int64 x = unchecked(a + b);
			if (((a ^ ~b) & (a ^ x) | (x & -x)) < 0)
				AdditionOverflow();

			return x;
		}


		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		// Special addition code that assumes b to be positive and less than 32 bits in size
		// and throws range exception on overflow
		internal static Int64 AddSpecial(Int64 a, Int64 b)
		{
			Int64 x = unchecked(a + b);
			if (x < a)
				ThrowOutOfRange();

			return x;
		}

		internal static Int64 Negate(Int64 x)
		{
			return -x;
			//return Int64.MinValue == x ? Int64.MaxValue : Int64.MaxValue == x ? Int64.MinValue : -x;
		}

		internal static Int64 Abs(Int64 x)
		{
			return Math.Abs(x);
			//return Int64.MinValue == x ? Int64.MaxValue : Math.Abs(x);
		}
	}
}