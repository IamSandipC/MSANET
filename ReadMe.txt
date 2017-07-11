Once you log-in to your VM make sure
1. You check proxy is setup correctly in Internet Explorer & "Bypass proxy Server for Local addresses" is checked

2. Give yourself "Full Control" on the folder C:\Training\DotNet (and all its sub folders - should be by default)

3. Run the dependencies:
	Double click on the C:\Training\DotNet\Dependencies\run-dependencies
	This will start the Eureka (port 8761), Config (port 8888) & Zipkin (port 9411) Servers

4. Run the application:
	Open the C:\Training\DotNet\SRC\MenuCatalog\MenuCatalog.sln in Visual Studio 2017
	Set all the projects as startup project from "properties" of "Solution"
	Run the application from Visual Studio

5. Access the application:
	Use Postman 
	Issue the following request to get the token for authentication
		POST to localhost:5000/connect/token
		In the body
			client_id:client
			client_secret:secret
			grant_type:password
			username:alice
			password:password
	Copy the "access_token" from response 
	Again in the postman resuest the MenuRecommender/MenuOfTheMoment API
		GET to http://localhost:50776/api/MenuRecommender/MenuOfTheMoment 
		In thye header set
			Authorization = bearer access_token
	Hit few times the URI 

6. Tracing can be seen at http://localhost:9411
7. If you want to change any of the config file in C:\Training\DotNet\SRC\MenuCatalog\Config folder, make sure to copy the same to C:\Training\DotNet\Config
