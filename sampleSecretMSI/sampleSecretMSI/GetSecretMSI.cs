using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Threading.Tasks;
using System.Configuration;
using System.Management.Automation;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.ServiceBus.Messaging;

namespace sampleSecretMSI
{
    [Cmdlet(VerbsCommon.Get, "Secret")]
    public class GetSecretCmdlet : PSCmdlet
    {
        private string _secretURI;
        private string _queueName;
        private string secret;

        [Parameter(Mandatory = true, HelpMessage = "The URI of the secret in the Key Vault")]
        [ValidateNotNullOrEmpty]
        public string secretURI
        {
            get { return _secretURI; }
            set { _secretURI = value; }
        }

        [Parameter(Mandatory = true, HelpMessage = "The name of the Service Bus queue")]
        [ValidateNotNullOrEmpty]
        public string queueName
        {
            get { return _queueName; }
            set { _queueName = value; }
        }
        protected override void BeginProcessing()
        {
            base.BeginProcessing();

        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            secret = GetSecretAsync(secretURI).Result;
            string valueSecret = "The secret value is: " + secret;
            WriteObject(valueSecret);

            var client = QueueClient.CreateFromConnectionString(secret, queueName);
            string sentMessage = EnqueueMessage(client);
            WriteObject(sentMessage);

            var messageRec = client.Receive();
            if (messageRec != null)
            {
                string retMessage = String.Format("Message id: {0}", messageRec.MessageId) + " " + String.Format("Message body: {0}", messageRec.GetBody<String>());
                WriteObject(retMessage);
            }

        }

        protected override void StopProcessing()
        {
            base.StopProcessing();
        }

        private static async Task<string> GetSecretAsync(string secretURI)
        {
            //Use this method if not running on VM
            //AzureServiceTokenProvider azureServiceTokenProvider =
            //                            new AzureServiceTokenProvider(ConfigurationManager.AppSettings["AzureServicesAuthConnString"]);

            AzureServiceTokenProvider azureServiceTokenProvider =
                            new AzureServiceTokenProvider("AuthenticateAs = App"); 

            var kv = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
          
            string secretUriSettings = ""; 

            SecretBundle secret;

            if (!String.IsNullOrEmpty(secretUriSettings))
            {
                secret = await kv.GetSecretAsync(secretUriSettings);
            }
            else
            {
                secret = await kv.GetSecretAsync(secretURI);
            }
                        
            return secret.Value;

        }

        private static string EnqueueMessage(QueueClient client)
        {
            var message = new BrokeredMessage("This is a test message!");
            client.Send(message);
            return "Message is sent to queue";
        }

    }
}
