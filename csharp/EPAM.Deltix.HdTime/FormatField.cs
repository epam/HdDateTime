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
