using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MenuRecommendation.Models;
using System.Net.Http;
using Microsoft.Extensions.Options;
using MenuRecommendation.Options;
using Steeltoe.Discovery.Client;
using Polly;
using Polly.Timeout;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Fallback;
using System.Net;
using Polly.Wrap;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;

namespace MenuRecommendation.Services
{
    public class MenuCatalogService : BaseService, IMenuCatalogService
    {

        readonly string menuServiceURI;
		readonly string appName;

		//Needs to be global since this policy to be maintained across service calls
		private CircuitBreakerPolicy<HttpResponseMessage> getMenusCircuitBreakerPolicy;
		private IHttpContextAccessor httpContextAccessor;
    


        public MenuCatalogService(IOptions<ServiceOptions> serviceOptions, IOptions<AppInfoOptions> appInfoOptions, IDiscoveryClient client, IHttpContextAccessor httpContextAccessor) : base(client)
        {
            menuServiceURI = serviceOptions.Value.MenuCatalog;
			appName = appInfoOptions.Value.Name;
			this.httpContextAccessor = httpContextAccessor;

			//configuring Polly to break the circuit
			getMenusCircuitBreakerPolicy = Policy.Handle<Exception>()
            .OrResult<HttpResponseMessage>(r => !(r.IsSuccessStatusCode))
               .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(200)
             );
        }


        public async Task<List<Menu>> GetMenus()
        {

            var request = new HttpRequestMessage(HttpMethod.Get, menuServiceURI);

			var accessToken = await httpContextAccessor.HttpContext.Authentication.GetTokenAsync("access_token");
			if (!string.IsNullOrEmpty(accessToken))
			{
				request.Headers.Add("AUTHORIZATION", " bearer " + accessToken);
			}

			var policyWrap = BuildGetMenusPolicy();

			var menus = await Invoke<List<Menu>>(request, policy: policyWrap, serviceName: appName);

            return menus;
        }

		private Policy<HttpResponseMessage> BuildGetMenusPolicy()
		{
			TimeoutPolicy httpTimeoutPolicy;
			RetryPolicy<HttpResponseMessage> httpRetryPolicy;
			FallbackPolicy<HttpResponseMessage> httpFallbackPolicy;
			PolicyWrap<HttpResponseMessage> policyWrap;

			//configuring Polly to check for timeout
			httpTimeoutPolicy = Policy.TimeoutAsync(3, TimeoutStrategy.Pessimistic);


			//configuring Polly to retry if circuit is not broken
			httpRetryPolicy = Policy.Handle<Exception>(r => !(r is BrokenCircuitException))
				.OrResult<HttpResponseMessage>(r => !(r.IsSuccessStatusCode))
			  .WaitAndRetryAsync(1,
			   retryAttempt => TimeSpan.FromSeconds(1));


			//configuring Polly for a fallback
			httpFallbackPolicy = Policy
				.Handle<Exception>()
			   .OrResult<HttpResponseMessage>(r => !(r.IsSuccessStatusCode))
				.FallbackAsync(
					 fallbackAction: (ct) => {
						 var httpResponse = new HttpResponseMessage();
						 httpResponse.Content = new StringContent(JsonConvert.SerializeObject(new List<Menu>() { new Menu() { ID = 0, Name = "Default Menu" } }));
						 return Task.FromResult<HttpResponseMessage>(httpResponse);
					 }
				);


			policyWrap = httpFallbackPolicy.WrapAsync(getMenusCircuitBreakerPolicy).WrapAsync(httpRetryPolicy).WrapAsync(httpTimeoutPolicy);

			return policyWrap;

		}
	}
}
