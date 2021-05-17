// =============================================================================
// Copyright 2021 EPAM Systems, Inc
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership. Licensed under the Apache License,
// Version 2.0 (the "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the
// License for the specific language governing permissions and limitations under
// =============================================================================
using System;

namespace EPAM.Deltix.HdTime
{
	internal abstract class FormatField
	{
		public override bool Equals(Object other)
		{
			return this == other || other is FormatField && other.GetHashCode() == this.GetHashCode();
		}

		public override int GetHashCode()
		{
			// TODO: Check returned value for different field types
			return GetType().GetHashCode();
		}
	}

	internal abstract class StaticFormatField : FormatField
	{
	}
}
