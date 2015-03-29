using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BestHTTP.Extensions
{
    /// <summary>
    /// Used for parsing WWW-Authenticate headers:
    /// Digest realm="my realm", nonce="4664b327a2963503ba58bbe13ad672c0", qop=auth, opaque="f7e38bdc1c66fce214f9019ffe43117c"
    /// </summary>
    public sealed class WWWAuthenticateHeaderParser : KeyValuePairList
    {
        public WWWAuthenticateHeaderParser(string headerValue)
        {
            Values = ParseQuotedHeader(headerValue);
        }

        private List<KeyValuePair> ParseQuotedHeader(string str)
        {
            List<KeyValuePair> result = new List<KeyValuePair>();

            if (str != null)
            {

                int idx = 0;

                // Read Type (Basic|Digest)
                string type = str.Read(ref idx, (ch) => !char.IsWhiteSpace(ch) && !char.IsControl(ch)).TrimAndLower();
                result.Add(new KeyValuePair(type));

                // process the rest of the text
                while (idx < str.Length)
                {
                    // Read key
                    string key = str.Read(ref idx, '=').TrimAndLower();
                    KeyValuePair qp = new KeyValuePair(key);

                    // Skip any white space
                    str.SkipWhiteSpace(ref idx);

                    qp.Value = str.ReadQuotedText(ref idx);

                    result.Add(qp);
                }
            }
            return result;
        }
    }
}