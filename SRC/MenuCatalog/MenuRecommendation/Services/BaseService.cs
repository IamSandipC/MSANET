using Criteo.Profiling.Tracing.Middleware;
using Newtonsoft.Json;
using Polly;
using Polly.Wrap;
using Steeltoe.Discovery.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MenuRecommendation.Services
{
    public abstract class BaseService
    {
       protected DiscoveryHttpClientHandler _handler;

        public BaseService(IDiscoveryClient client)
        {
           _handler = new DiscoveryHttpClientHandler(client);
        }

		public virtual async Task<T> Invoke<T>(HttpRequestMessage request, object content = null, Policy<HttpResponseMessage> policy = null, String serviceName = "")
		{
			var client = GetClient(serviceName);

			try
			{
                if (content != null)
                {
                    request.Content = Serialize(content);
                }
                using (HttpResponseMessage response = policy == null
                                                      ? await client.SendAsync(request)
                                                      :
                                                      await policy.ExecuteAsync(async () =>
                                                      {
														  var requestMsg = await CloneHttpRequestMessageAsync(request);
														 
														  return await client.SendAsync(requestMsg);

														  //var requestMsg = new HttpRequestMessage(request.Method, request.RequestUri);
														  //requestMsg.Content = request.Content;
														  //return client.SendAsync(requestMsg);
													  }))
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    return Deserialize<T>(stream);
                }
            }
            catch (Exception e)
            {
                //log error
            }

            return default(T);
        }

        public virtual T Deserialize<T>(Stream stream)
        {
            using (JsonReader reader = new JsonTextReader(new StreamReader(stream)))
            {
                JsonSerializer serializer = new JsonSerializer();
                return (T)serializer.Deserialize(reader, typeof(T));
            }

        }

        public virtual HttpContent Serialize(object toSerialize)
        {
            string json = JsonConvert.SerializeObject(toSerialize);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

		public virtual HttpClient GetClient(string serviceName = "")
		{

			var client = new HttpClient(new TracingHandler(serviceName, _handler), false);
			return client;
		}

		public static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage req)
		{
			HttpRequestMessage clone = new HttpRequestMessage(req.Method, req.RequestUri);

			// Copy the request's content (via a MemoryStream) into the cloned object
			var ms = new MemoryStream();
			if (req.Content != null)
			{
				await req.Content.CopyToAsync(ms).ConfigureAwait(false);
				ms.Position = 0;
				clone.Content = new StreamContent(ms);

				// Copy the content headers
				if (req.Content.Headers != null)
					foreach (var h in req.Content.Headers)
						clone.Content.Headers.Add(h.Key, h.Value);
			}


			clone.Version = req.Version;

			foreach (KeyValuePair<string, object> prop in req.Properties)
				clone.Properties.Add(prop);

			foreach (KeyValuePair<string, IEnumerable<string>> header in req.Headers)
				clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

			return clone;
		}

	}
}
