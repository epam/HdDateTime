using System;
using System.Collections.Generic;
using System.Text;

namespace EPAM.Deltix.HdTime
{
	class FormatError : Exception
	{
		public FormatError(String str)
			: base(str)
		{
		}
	}
}
