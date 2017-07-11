using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System.Security.Claims;

namespace AuthService
{
    public class Config
    {
		public static IEnumerable<ApiResource> GetApiResources()
		{
			return new List<ApiResource>
			{
				new ApiResource("menu_system_APIs", "Menu System APIS",
				new [] { "name","role"}
				)
			};
		}

		public static IEnumerable<Client> GetClients()
		{
			return new List<Client>
			{
				new Client
				{
					ClientId = "client",

                    AllowedGrantTypes =GrantTypes.List( 
						GrantType.ClientCredentials,
						GrantType.ResourceOwnerPassword,
						GrantType.Implicit
						),

                    // secret for authentication
                    ClientSecrets =
					{
						new Secret("secret".Sha256())
					},

                    // scopes that client has access to
                    AllowedScopes = { "menu_system_APIs" }

				}
			};
		}

		public static List<TestUser> GetUsers()
		{
			return new List<TestUser>
			{
				new TestUser
				{
					SubjectId = "1",
					Username = "alice",
					Password = "password",
					Claims=new [] {
						new Claim("role", "guest"),
						new Claim("name", "alice")
					}
				},
				new TestUser
				{
					SubjectId = "2",
					Username = "bob",
					Password = "password",
					 Claims= new []{
						new Claim("role", "admin"),
						new Claim("name", "bob")
					}
				}
			};
		}
	}
}
