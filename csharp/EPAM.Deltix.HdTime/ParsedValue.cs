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

namespace EPAM.Deltix.HdTime
{
	internal struct ParsedValue
	{
		public long x;         // Complete value without sign
		public sbyte sign;     // Sign mask (0 or -1)

		public int year;       // Year, always 4 digit integer
		public int month;      // Month of year, [0..11] !!
		public int day;        // Day of month. [1..31]

		public long GetTs()
		{
			long x = this.x;
			long mask = this.sign;
			return x + mask ^ mask;
		}

		public long GetDt()
		{
			return Convert.DateTime.From(year, month, day) + x;
		}

		public void ResetTs()
		{
			x = 0;
			sign = 0;
		}

		public void ResetDt()
		{
			x = 0;
			sign = 0;
			year = month = day = 1;
		}
	}
}
