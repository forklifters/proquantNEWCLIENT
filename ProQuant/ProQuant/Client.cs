using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.IO;

namespace ProQuant
{
    public class Client
    {

        public static async Task<string> client(string authType, string user, string pass)
        {
            string encodedUP = Base64Encode(user + ":" + pass);
            //string auth = "Basic " + encodedUP;

            string key = "/api/api/5?id=cmd$~gettoken";
            string add = $"https://pqapi.co.uk:58330{key}";
            List<string> TokenInfo = new List<string>();
            HttpClient _client = new HttpClient();

            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(authType, encodedUP);

            string content;
            try
            {
                HttpResponseMessage response = await _client.GetAsync(add);
                response.EnsureSuccessStatusCode();

                content = await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException)
            {
                content = "errorerrorerror";
            }

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

            string content;
            try
            {
                var response = await _client.GetAsync(add, HttpCompletionOption.ResponseContentRead);
                response.EnsureSuccessStatusCode();

                content = await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException)
            {
                content = "errorerrorerror";
            }
            
            return content;
        }


        public static async Task<string> GET(string token, string key)
        {

            string add = $"https://pqapi.co.uk:58330{key}";
            //string add = $"https://proq.remotewebaccess.com:58330{key}"; - OLD API

            HttpClient _getClient = new HttpClient();
            

            _getClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            string content;
            try
            {
                var response = await _getClient.GetAsync(add);
                response.EnsureSuccessStatusCode();
                content = await response.Content.ReadAsStringAsync();
                var x = content;
            }
            catch (HttpRequestException)
            {
                content = "errorerrorerror";
            }

            return content;
        }


        public static async Task<string> Post(string token, string key, string jsonContent)
        {

            string add = $"https://pqapi.co.uk:58330{key}";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(add);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Accept = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {token}");
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(jsonContent);
                streamWriter.Flush();
                streamWriter.Close();
            }
            

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                var x = result;

                return result;
            }       
        }


        public static async Task<string> GETnoAuth(string key)
        {

            string add = $"https://pqapi.co.uk:58330{key}";
            //string add = $"https://proq.remotewebaccess.com:58330{key}"; - OLD API

            HttpClient _getClient = new HttpClient();

            string content;
            try
            {
                var response = await _getClient.GetAsync(add);
                response.EnsureSuccessStatusCode();

                content = await response.Content.ReadAsStringAsync();

            }
            catch (HttpRequestException)
            {
                content = "errorerrorerror";
            }

            return content;
        }


        public static async Task<string> GETChangePassword(string passwordKey, string user, string key)
        {

            string add = $"https://pqapi.co.uk:58330{key}";
            string _passwordKey = Base64Encode(user + ":" + passwordKey);
            //string add = $"https://proq.remotewebaccess.com:58330{key}"; - OLD API

            HttpClient _getClient = new HttpClient();


            _getClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", _passwordKey);

            string content;
            try
            {
                var response = await _getClient.GetAsync(add);
                response.EnsureSuccessStatusCode();

                content = await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException)
            {
                content = "errorerrorerror";
            }

            return content;
        }




        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

    }
}
