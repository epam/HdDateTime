/*
 * Copyright 2021 EPAM Systems, Inc
 *
 * See the NOTICE file distributed with this work for additional information
 * regarding copyright ownership. Licensed under the Apache License,
 * Version 2.0 (the "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the
 * License for the specific language governing permissions and limitations under
 * the License.
 */
package com.epam.deltix.hdtime;

class FormatString {
    public interface Target {
        /**
         * Return max field length for the specified char. <= 0 if not a field
         * @param fieldChar field identifier character
         * @return maximum field length before padding is applied, returns value <= 0 if not a field identifier
         */
        int fieldLength(char fieldChar);

        /**
         * Return padding character for the field
         * Only called if fieldLength(fieldChar) > 0 && encountered length > fieldLength(fieldChar)
         * @param fieldChar field identifier character
         * @return
         */
        char paddingForField(char fieldChar);
        void addField(char fieldChar, int length) throws FormatError;
        void addString(CharSequence str, int ofs, int n);

        default void addString(CharSequence str) {
            addString(str, 0, str.length());
        }
    }

    static void parse(final String fmt, final Target target) {
        int fmtLength = fmt.length();
        StringBuilder sb = new StringBuilder(fmtLength);
        boolean literalMode = false, wasQuote = false;

        for (int i = 0; i < fmtLength;) {
            char c = fmt.charAt(i++);
            if ('\'' == c) {
                literalMode ^= true;
                if (false == (wasQuote ^= true))
                    sb.append('\'');

                continue;
            }

            wasQuote = false;
            int maxLen;
            if (!literalMode && (maxLen = target.fieldLength(c)) > 0) {
                int n = i;
                for (; n < fmtLength && fmt.charAt(n) == c; ++n) {}
                n -= i;
                i += n++;
                if (n > maxLen) {
                    char padding = target.paddingForField(c);
                    for (int j = 0; j < n - maxLen; j++)
                        sb.append(padding);

                    n = maxLen;
                }

                if (sb.length() > 0) {
                    target.addString(sb);
                    sb.setLength(0);
                }

                target.addField(c, n);
            } else {
                sb.append(c);
            }
        }

        if (sb.length() > 0)
            target.addString(sb);
    }
}