using System;
using System.Collections.Generic;
using System.Text;

using System.Threading.Tasks;
using Refit;
using System.Net;

namespace ProQuant
{        
    public interface IMakeUpApi
    {

        [Get("/{key}")]
        Task<string> GetKey(string key, [Header("Authorization")] string authorisation);

        [Get("/{key}")]
        Task<string> GetToken(string key, [Header("Authorization")] string basicauth);

    }   
}
