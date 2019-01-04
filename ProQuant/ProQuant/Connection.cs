using System;
using System.Collections.Generic;
using System.Text;

namespace ProQuant
{
    public class Connection
    {
        public string Token { get; set; }
        public string User { get; set; }
        public string Pass { get; set; }

        public string FirebaseToken { get; set; }
        public string APNSToken { get; set; }

        public string ID { get; set; }
        public string MD { get; set; }

        public string Name { get; set; }
        public TokenInfo TokenInfoJsonProps { get; set; }
        public string FirebaseObject { get; set; }
    }
}
