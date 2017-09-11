using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace CameraStarterKit
{
    public static class FaceDetection
    {
        public const string groupKey = "ordina_innovation_day_2017";
        public const string subKey = "12cddee097d7403eb868a49f31249756";

        private static HttpClient client = new HttpClient();

        private static void setup()
        {
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subKey);
        }

        public static async Task CreateGroup()
        {
            setup();

            var uri = "https://westeurope.api.cognitive.microsoft.com/face/v1.0/persongroups/" + groupKey;
            byte[] byteData = Encoding.UTF8.GetBytes("{ Name: 'Ordina Innovation day 2017' }");

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await client.PutAsync(uri, content);

                // 409 = conflict
                // 200 = ok
                if (response.StatusCode.ToString() == "409")
                {
                    // Conflict, meaning we have already created this group.
                }
            }
        }

        public static async Task<Person> CreatePerson()
        {
            setup();

            var uri = "https://westeurope.api.cognitive.microsoft.com/face/v1.0/persongroups/" + groupKey + "/persons";
            byte[] byteData = Encoding.UTF8.GetBytes("{ Name: 'S. Pongebob' }");

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await client.PostAsync(uri, content);

                // 409 = conflict
                // 200 = ok
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // Created a new person, return the id;
                    var o = await response.Content.ReadAsStringAsync();
                    Person p = JsonConvert.DeserializeObject<Person>(o);
                    return p;
                }
            }

            return null;
        }

        public static async Task<byte[]> Convert(IRandomAccessStream s)
        {
            var dr = new DataReader(s.GetInputStreamAt(0));
            var bytes = new byte[s.Size];
            await dr.LoadAsync((uint)s.Size);
            dr.ReadBytes(bytes);
            return bytes;
        }
    }

    public class Person
    {
        public string personId;
        public Dictionary<string, string> faces;

        public async Task<AddedFace> AddFace(byte[] data)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", FaceDetection.subKey);

            var uri = "https://westeurope.api.cognitive.microsoft.com/face/v1.0/persongroups/" + FaceDetection.groupKey + "/persons/" + personId + "/persistedFaces";

            using (var content = new ByteArrayContent(data))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                var response = await client.PostAsync(uri, content);

                if (response.IsSuccessStatusCode)
                {
                    var o = await response.Content.ReadAsStringAsync();
                    AddedFace face = JsonConvert.DeserializeObject<AddedFace>(o);
                    return face;
                }
            }
            return null;
        }

        public async Task RemoveFace(string faceId)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", FaceDetection.subKey);

            var uri = "https://westeurope.api.cognitive.microsoft.com/face/v1.0/persongroups/" + FaceDetection.groupKey + "/persons/" + personId + "/persistedFaces/" + faceId;

            var response = await client.DeleteAsync(uri);
        }
    }

    public class AddedFace
    {
        public string PersistedFaceId;
    }
}
