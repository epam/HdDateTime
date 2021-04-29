using System;

namespace EPAM.Deltix.HdTime
{
	class ParseException : Exception
	{
		public ParseException(String str, int i)
			: base(i < str.Length
				? String.Format("Unable to parse: '{0}[{1}]'", str.Substring(0, i), str.Substring(i))
				: String.Format("Unable to parse: '{0}' at index {1}", str, i))
		{
		}
	}
}
