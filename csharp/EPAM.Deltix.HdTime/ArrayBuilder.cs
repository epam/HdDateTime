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
 