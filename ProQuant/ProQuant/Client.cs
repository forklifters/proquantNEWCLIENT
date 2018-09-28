using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;

namespace ProQuant
{
    public class Client
    {

        public static async Task<string> client(string authType, string user, string pass)
        {
            string encodedUP = Base64Encode(user + ":" + pass);
            string auth = "Basic " + encodedUP;

            string key = "/api/api/5?id=cmd$~gettoken";
            string add = $"https://pqapi.co.uk:58330{key}";
            List<string> TokenInfo = new List<string>();
            HttpClient _client = new HttpClient();

            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(authType, encodedUP);

            HttpResponseMessage response = await _client.GetAsync(add);
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            var x = content;

            return content;
        }

        public static async Task<string> GET_Token(string key, string authType, string user, string pass)
        {
            string encodedUP = Base64Encode(user + ":" + pass);
            //string auth = "Basic " + encodedUP;

            string add = $"https://pqapi.co.uk:58330{key}";
            //string add = $"https://proq.remotewebaccess.com:58330{key}";
            HttpClient _client = new HttpClient();

            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(authType, encodedUP);

            var response = await _client.GetAsync(add, HttpCompletionOption.ResponseContentRead);
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            string x = content;

            return content;
        }

        public static async Task<string> GET(string token, string key)
        {

            string add = $"https://pqapi.co.uk:58330{key}";
            //string add = $"https://proq.remotewebaccess.com:58330{key}"; - OLD API

            HttpClient _getClient = new HttpClient();
            

            _getClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _getClient.GetAsync(add);
            
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            string x = content;
            Console.WriteLine("#######################################################\n" +
                              "#######################################################\n" +
                              "###############       GOT CONTENT       ###############\n" +
                              "#######################################################\n" +
                              "#######################################################");

            
            return content;
        }

        public static async Task<string> GETnoAuth(string key)
        {

            string add = $"https://pqapi.co.uk:58330{key}";
            //string add = $"https://proq.remotewebaccess.com:58330{key}"; - OLD API

            HttpClient _getClient = new HttpClient();

            var response = await _getClient.GetAsync(add);

            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            string x = content;


            return content;
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

    }
}
