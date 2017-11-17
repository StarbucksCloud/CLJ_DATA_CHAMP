using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Table; // Namespace for Table storage types
using System.Collections;


using System.IO;
using System.Net;
using System.Net.Http;
using System.Configuration;
using System.Reflection;
using System.Xml.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Threading;

//using Microsoft.Azure.Management.ServiceBus;
using Microsoft.Azure.ServiceBus;
//Install-Package Microsoft.Azure.ServiceBus -Version 2.0.0

namespace CLJ_HACK
{
    public partial class Categories : System.Web.UI.Page
    {
        const string ServiceBusConnectionString = "Endpoint=sb://fidosb.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=3/cY9RXnddiQQL10hXR33YdYOOypVIoKnUEjG+ZwW3o=";
        const string TopicName = "fidoscattersearch";
        const string SubscriptionName = "fidosubscription";
        static ITopicClient topicClient;
        static ISubscriptionClient subscriptionClient;

        protected void Page_Load(object sender, EventArgs e)
        {




        }

        public void ShowData(Object sender, EventArgs e)
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference("FIDODATAMASTER");

            //TableQuery<StoreEntity> rangeQuery = new TableQuery<StoreEntity>().Where(TableQuery.GenerateFilterCondition("DataEntity",
            //   QueryComparisons.Equal, "DayEndInventory"));

            TableQuery<StoreEntity> rangeQuery = new TableQuery<StoreEntity>();

            ArrayList myList = new ArrayList();
            string strInventory = "", strLabor = "", strSales = "", strPartner = "";
            if (chkInventory.Checked)
            {
                strInventory = TableQuery.GenerateFilterCondition("DataEntity", QueryComparisons.Equal, "DayEndInventory");
                myList.Add(strInventory);
                
            }
            if (chkLabor.Checked)
            {
                strLabor = TableQuery.GenerateFilterCondition("DataEntity", QueryComparisons.Equal, "Labor");
                myList.Add(strLabor);
            }
            if (chkSales.Checked)
            {
                strSales = TableQuery.GenerateFilterCondition("DataEntity", QueryComparisons.Equal, "Sales");
                myList.Add(strSales);
            }
            if (chkPartner.Checked)
            {
                strPartner = TableQuery.GenerateFilterCondition("DataEntity", QueryComparisons.Equal, "Partner");
                myList.Add(strPartner);
            }

            string strfinalfilter = "";

            switch(myList.Count)
            {
                case 1:
                    strfinalfilter = myList[0].ToString();
                    break;
                case 2:
                    strfinalfilter = TableQuery.CombineFilters(myList[0].ToString(), TableOperators.Or, myList[1].ToString());
                    break;
                case 3:
                    strfinalfilter = TableQuery.CombineFilters(TableQuery.CombineFilters(myList[0].ToString(), TableOperators.Or, myList[1].ToString()), TableOperators.Or, myList[2].ToString());
                    break;
                case 4:
                    strfinalfilter = TableQuery.CombineFilters(TableQuery.CombineFilters(TableQuery.CombineFilters(myList[0].ToString(), TableOperators.Or, myList[1].ToString()), TableOperators.Or, myList[2].ToString()), TableOperators.Or, myList[3].ToString());
                    break;
            }
            
            //string strfinalfilter = TableQuery.CombineFilters(TableQuery.CombineFilters(TableQuery.CombineFilters(strInventory, TableOperators.And, strLabor), TableOperators.And, strSales), TableOperators.And, strPartner);


            rangeQuery.FilterString = strfinalfilter;
            //// Print the fields for each customer.
            //foreach (StoreEntity entity in table)
            //{
            //    Console.WriteLine("{0}, {1}\t{2}\t{3}", entity.PartitionKey, entity.RowKey,
            //        entity.Email, entity.PhoneNumber);
            //}

            grdData.DataSource = table.ExecuteQuery(rangeQuery); //.Take(5)  to take only 5 records
            grdData.DataBind();

        }

        public void POST_RESTRequest(Object sender, EventArgs e)
        {

            string strURL = "https://fidosb.servicebus.windows.net/fidoscattersearch";


            //Post(strURL, GetJSONString());

            POST_ServiceBus(strURL);



        }

        public static string Post(string url, string postData)
        {
            WebRequest request = WebRequest.Create(url);
            request.Method = "POST";
            request.Headers.Set(HttpRequestHeader.Authorization, "3/cY9RXnddiQQL10hXR33YdYOOypVIoKnUEjG+ZwW3o=");
            request.Headers.Add("Account", "RootManageSharedAccessKey");
            request.Headers.Add("ServiceBusAuthorization", "SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=3/cY9RXnddiQQL10hXR33YdYOOypVIoKnUEjG+ZwW3o=");



            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response = request.GetResponse();
            dataStream = response.GetResponseStream();
            var reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();
            response.Close();

            return responseFromServer;
        }

        public void POST_ServiceBus(string strURL)
        {


            MainAsync().GetAwaiter().GetResult();

        }

        static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            // Process the message
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

            // Complete the message so that it is not received again.
            // This can be done only if the subscriptionClient is opened in ReceiveMode.PeekLock mode (which is default).
            await subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
        }
        // Use this Handler to look at the exceptions received on the MessagePump
        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            return Task.CompletedTask;
        }

        static void RegisterOnMessageHandlerAndReceiveMessages()
        {
            // Configure the MessageHandler Options in terms of exception handling, number of concurrent messages to deliver etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of Concurrent calls to the callback `ProcessMessagesAsync`, set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
                AutoComplete = false
            };

            // Register the function that will process messages
            subscriptionClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }
        static async Task SendMessagesAsync(int numberOfMessagesToSend)
        {
            for (var i = 0; i < numberOfMessagesToSend; i++)
            {
                try
                {
                    // Create a new message to send to the topic
                    //string messageBody = $"Message {i}";

                    string messageBody = GetJSONString();
                    var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                    // Write the body of the message to the console
                    Console.WriteLine($"Sending message: {messageBody}");

                    // Send the message to the topic
                    await topicClient.SendAsync(message);
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
                }
            }
        }

        static async Task MainAsync()
        {
            const int numberOfMessages = 10;
            topicClient = new TopicClient(ServiceBusConnectionString, TopicName);
            subscriptionClient = new SubscriptionClient(ServiceBusConnectionString, TopicName, SubscriptionName);

            Console.WriteLine("======================================================");
            Console.WriteLine("Press any key to exit after receiving all the messages.");
            Console.WriteLine("======================================================");

            // Register Subscription's MessageHandler and receive messages in a loop
            RegisterOnMessageHandlerAndReceiveMessages();

            // Send Messages
            await SendMessagesAsync(numberOfMessages);

            Console.ReadKey();

            await subscriptionClient.CloseAsync();
            await topicClient.CloseAsync();
        }


        public void Post_Temp(string strURL)
        {



            var httpWebRequest = (HttpWebRequest)WebRequest.Create(strURL);

            ASCIIEncoding encoder = new ASCIIEncoding();

            var serializedObject = JObject.Parse(GetJSONString());

            byte[] data = encoder.GetBytes(GetJSONString()); // a json object, or xml, whatever...

            //httpWebRequest.ContentType = "application/xml";
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.Method = "POST";

            httpWebRequest.ContentType = "application/json";
            httpWebRequest.ContentLength = data.Length;
            httpWebRequest.Expect = "application/json";
            

            /*
            byte[] credentialBuffer =
                new UTF8Encoding().GetBytes(
                 username + ":" +
                apiToken);


            httpWebRequest.Headers["Authorization"] =
               "Basic " + Convert.ToBase64String(credentialBuffer);

            httpWebRequest.PreAuthenticate = true;
            */

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                Console.WriteLine(result);

            }


        }

        public static string GetJSONString()
        {


            string strJSON = "";



            strJSON = "{" +
                            "\"correlationId\": \"" + Guid.NewGuid().ToString() + "\"," +
                             "\"storeId\": \"11111\"," +
                            "\"dataSources\" : [" +
                               "{\"name\": \"sales\"," +
                                  "\"parameters\": [" +
                                        "{" +
                                            "\"key\": \"quantity\"," +
                                            "\"operator\": \"equals\"," +
                                            "\"value\": \"5\"" +
                                        "}" +
                                    "]" +
                               "}," +
                               "{\"name\": \"inventory\"," +
                                   "\"parameters\": [" +
                                       "{" +
                                           "\"key\": \"quantity\"," +
                                           "\"operator\": \"equals\"," +
                                           "\"value\": \"5\"" +
                                       "}" +
                                   "]" +
                               "}," +
                               "{\"name\": \"labor\"," +
                                   "\"parameters\": [" +
                                       "{" +
                                           "\"key\": \"quantity\"," +
                                           "\"operator\": \"equals\"," +
                                           "\"value\": \"5\"" +
                                       "}" +
                                   "]" +
                               "}" +
                            "]" +
                            "}";

            return strJSON;
        }


        public class StoreEntity : TableEntity
        {
            public StoreEntity(string strPartitionKey, string strRowKey, string strDataEntity)
            {
                this.PartitionKey = strPartitionKey;
                this.RowKey = strRowKey;
                DataEntity = strDataEntity;
            }

            public StoreEntity() { }

            public string DataEntity { get; set; }

            //public string PhoneNumber { get; set; }
        }





    }
}