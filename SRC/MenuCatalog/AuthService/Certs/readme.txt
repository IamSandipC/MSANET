JWT token can be verified at 
http://jwt.calebb.net/ 

Create new cert if required
makecert -n "CN=AuthSample" -a sha256 -sv IdentityServer4Auth.pvk -r IdentityServer4Auth.cer
pvk2pfx -pvk IdentityServer4Auth.pvk -spc IdentityServer4Auth.cer -pfx IdentityServer4Auth.pfx