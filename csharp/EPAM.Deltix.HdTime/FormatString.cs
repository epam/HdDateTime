using System;
using System.Text;

namespace EPAM.Deltix.HdTime
{
	internal class FormatString
	{
		internal interface ITarget
		{
			/**
			 * Return max field length for the specified char. <= 0 if not a field
			 * @param fieldChar field identifier character
			 * @return maximum field length before padding is applied, returns value <= 0 if not a field identifier
			 */

			int FieldLength(char fieldChar);

			/**
			 * Return padding character for the field
			 * Only called if fieldLength(fieldChar) > 0 && encountered length > fieldLength(fieldChar)
			 * @param fieldChar field identifier character
			 * @return
			 */
			char PaddingForField(char fieldChar);
			void AddField(char fieldChar, int length);
			void AddString(String str, int ofs, int n);
			void AddString(String str);
		}

		internal static void Parse(String fmt, ITarget target)
		{
			int fmtLength = fmt.Length;
			StringBuilder sb = new StringBuilder(fmtLength);
			bool literalMode = false, wasQuote = false;

			for (int i = 0; i < fmtLength;)
			{
				char c = fmt[i++];
				if ('\'' == c)
				{
					literalMode ^= true;
					if (false == (wasQuote ^= true))
						sb.Append('\'');

					continue;
				}

				wasQuote = false;
				int maxLen;
				if (!literalMode && (maxLen = target.FieldLength(c)) > 0)
				{
					int n = i;
					for (; n < fmtLength && fmt[n] == c; ++n) { }
					n -= i;
					i += n++;
					if (n > maxLen)
					{
						char padding = target.PaddingForField(c);
						for (int j = 0; j < n - maxLen; j++)
							sb.Append(padding);

						n = maxLen;
					}

					if (sb.Length > 0)
					{
						target.AddString(sb.ToString());
						sb.Length = 0;
					}

					target.AddField(c, n);
				}
				else
				{
					sb.Append(c);
				}
			}

			if (sb.Length > 0)
				target.AddString(sb.ToString());
		}
	}
}
