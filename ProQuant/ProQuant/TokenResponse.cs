using System;
using System.Collections.Generic;
using System.Text;

namespace ProQuant
{
    class TokenResponse
    {
        public string Token { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }

        public static List<string> TokenParse(string tokenResponseInput)
        {
            List<string> tokenInfo = new List<string>();

            string[] rawInputParts = tokenResponseInput.Split('~');
            string token = rawInputParts[0].Replace("\"Token=", "");
            string id = rawInputParts[1].Replace("id=", "");
            string name = rawInputParts[2].Replace("name=", "").Replace("\"", "").Trim();


            tokenInfo.Add(token);
            tokenInfo.Add(id);
            tokenInfo.Add(name);

            return tokenInfo;
        }

    }
}
