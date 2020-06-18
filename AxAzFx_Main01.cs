using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Azure.Devices;//1.21, no preview by now
using Microsoft.Azure.Devices.Shared;

namespace Company.Function
{
    public static class HttpTriggerCSharp_examp01
    {
        [FunctionName("HttpTriggerCSharp_examp01")]
        
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,ILogger log)
        {
            //string connString = "HostName=IoTHubFirwar.azure-devices.net;DeviceId=Balanza1;SharedAccessKey=ui+Zh6pRIXZ8K60Xr9tvw6o7IvWRje4z4653DEltgYo=";    //DEVICE CONNECTION
            string connString = "HostName=IoTHubFirwar.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=vkGkB0bgjg3F46n+lOXl3Z8iYTxwoSzx6tIpxMYIftI=";    //SHARED CONNECTION STRING
            //
            RegistryManager registryManager;
            registryManager = RegistryManager.CreateFromConnectionString(connString);
            StartReboot().Wait();
            //
            
            /*
            client = ServiceClient.CreateFromConnectionString(connString);
            CloudToDeviceMethod method = new CloudToDeviceMethod("stop");
            method.ResponseTimeout = TimeSpan.FromSeconds(30);
            CloudToDeviceMethodResult result = await client.InvokeDeviceMethodAsync(targetDevice, method);
            */

            //string nombre = req.Query["name"];//haciendo GET
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            //string nombre = data.name;            //Esta parte es haciendo POST: ok
            //Esta parte es haciendo POST
            string firstname = data.firstname;//POST
            string lastname = data.lastname;//POST
            string nombre = firstname + " " + lastname;//POST
            //Postman: {"firstname":"Michel","lastname":"aguero"}
            //name =  name ?? (data?.name );//obtitene el valor de data.name //GET
            //string responseMessage = string.IsNullOrEmpty(nombre)? "BAD": $"Hello, {nombre}. This HTTP triggered function executed successfully.";
            //return new OkObjectResult(responseMessage);

            var str = "Server=tcp:sqlserver08062020.database.windows.net,1433;Initial Catalog=mysqlserverFirwar21;Persist Security Info=False;User ID=jcaf;Password=eklipse1812*;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            using (SqlConnection conn = new SqlConnection(str))
            {
                conn.Open();
                var text = "INSERT INTO dbo.Bal (Name) VALUES (\'" +nombre+ "\');";
                using (SqlCommand cmd = new SqlCommand(text, conn))
                {
                    // Execute the command and log the # rows affected.
                    var rows = await cmd.ExecuteNonQueryAsync();
                    log.LogInformation($"{rows} rows were updated");
                }
            }

            //ok
            Dictionary<string,string> resp = new Dictionary<string,string>();
		    resp.Add("name", nombre);
            string n = JsonConvert.SerializeObject(resp);
            return new OkObjectResult(n); //ok

        }

        public static async Task StartReboot()
        {
                                 
            string connString = "HostName=IoTHubFirwar.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=vkGkB0bgjg3F46n+lOXl3Z8iYTxwoSzx6tIpxMYIftI=";    
            ServiceClient client;
            string targetDevice = "Balanza1";


            client = ServiceClient.CreateFromConnectionString(connString);
            //CloudToDeviceMethod method = new CloudToDeviceMethod("stop");
            CloudToDeviceMethod method = new CloudToDeviceMethod("start");
            //CloudToDeviceMethod method = new CloudToDeviceMethod(firstname);
            
            method.ResponseTimeout = TimeSpan.FromSeconds(30);//15
            //New:
            method.ConnectionTimeout = TimeSpan.FromSeconds(15);

            CloudToDeviceMethodResult result = await client.InvokeDeviceMethodAsync(targetDevice, method);

            //Console.WriteLine("Invoked firmware update on device.");
        }

    }
}
