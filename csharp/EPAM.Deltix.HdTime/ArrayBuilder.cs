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
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace EPAM.Deltix.HdTime
{
	internal class ArrayBuilder<T>
	{
		internal readonly List<T> Items = new List<T>();

		public ArrayBuilder<T> Add(T f)
		{
			Items.Add(f);
			return this;
		}

		public T[] GetFields()
		{
			return Items.ToArray();
		}

		public virtual void Clear()
		{
			Items.Clear();
		}
	}
}
 