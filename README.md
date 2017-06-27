**Azure Template for creating Key Vault with a secret in it and a VM with MSI enabled**  
This Sample has one template which describes how to create a vault, create a service bus, put the service bus connection key as a secret in the vault and create a VM, which has MSI (Managed Service Identity) enabled. MSI makes authentication for developers much easier by abstracting away interaction with AAD (Azure Active Directory). The sample then shows how to call the Key Vault to retrieve the secret that you initially put in with authenicating using MSI. The code also shows how to put a message into the Service Bus queue that is created and get the message from the queue. The code is wrapped in a powerShell cmdlet.

**Step 1)**
Create using ARM template a Service Bus and create Key Vault with a secret created in the vault, which is the connection key for the Service Bus. In addition, create a VM that is MSI enabled and has access to the key vault that was created. This is all done using one template. 

**Step 2)**
Store the debug folder of last built project in a fileshare on the storage account you created with the template and run the following commands to include the PowerShell command and run it. The Get-Secret command will accept as a parameter a url to the secret. 

1. `cd C:\Users\yourUserName\`
2. `mkdir repoAzureStorage`
3. `net use k: \\nameofstorage.file.core.windows.net\fileshareName /u:AZURE\nameofstorage somekey`
4. `k:`
5. `dir`
6. `copy Debug.zip C:\Users\yourUserName\repoAzureStorage` (Please make sure the repoAzureStorage folder is already created before running copy)

Then in PowerShell run.

7. `Set-ExecutionPolicy unrestricted`
8. `Import-Module C:\Users\yourUserName\repoAzureStorage\Debug\sampleSecretMSI.dll -Verbose`
9. `Get-Secret` 

**Step 3)**
For Details on how the inner code in the PowerShell cmdlet works please refer to the following tutorial on using Key Vault from a VM that has MSI enabled. 

<https://securitytools.visualstudio.com/DefaultCollection/_git/ASAL?path=%2FAzureServicesFromMSIVM.md&version=GBmaster&_a=contents>

In Visual Studio, go to **Tools->Nuget Package Manager->Package Manager Console**, and run these commands in the Package Manager Console. It is very important the necessary nuget packages be downloaded:  


```Install-Package Microsoft.Azure.Services.AppAuthentication -Source "https://msazure.pkgs.visualstudio.com/_packaging/Official/nuget/v3/index.json"  -Prerelease```


OR


```Install-Package Microsoft.Azure.Services.AppAuthentication -Source "https://securitytools.pkgs.visualstudio.com/_packaging/ASAL/nuget/v3/index.json"  -Prerelease```

```Install-Package Microsoft.Azure.KeyVault```


**Explanation of Template Sections**

1. In the parameters section of the template you can see descriptions which will be displayed when the user is going to be prompted for inputs like:
    
    * adminUsername
    * adminPassword
    * windowsOSVersion
    * keyVaultName
    * tenantId - To get tenantId just run Get-AzureRmSubscription in PowerShell and right down the tenantId for the subscription you are using
    * objectId - To get objectId go to Portal -> AAD -> Users-all users -> search for your name and click on it then objectId will be displayed in the top left of the pane
    * keysPermissions
    * secretsPermissions
    * vaultSku
    * enabledForDeployment
    * enabledForTemplateDeployment
    * enableVaultForVolumeEncryption
    * serviceBusNamespaceName
    * serviceBusQueueName
    * serviceBusApiVersion

2. The variables section describes the names of important variables to be used for creating resources, the user is not prompted for these inputs:

    * storageAccountName
    * publicIPAddressName
    * addressPrefix
    * subnetName
    * subnetPrefix
    * publicIPAddressName
    * vmName
    * virtualNetworkName
    * subnetRef
    * vmIdentityResourceId
    * sbVersion
    * defaultSASKeyName
    * authRuleResourceId
    * secretName
    * location

3. The resources section specifies dependancies and describes what resources to use, which in this case are Service Bus, Key Vault, MSI and Storage Accounts. In addition, each of these resources can be split into their own templates and use to only create their respective components for other uses, such as simply creating a Key Vault or Service Bus.

    * Microsoft.KeyVault/vaults - This is the Key Vault where the ServiceBus Key will be placed as a secret note that in the "dependsOn" section serviceBusNamespaceName. Also notice how this first resource has a sub resource used for describing the type and location where the secret is to come from
    * Microsoft.ServiceBus/Namespaces - This ServiceBus Namespace is given the serviceBusNamespaceName specified in the parameters section as user input
    * Microsoft.Storage/storageAccounts - storage accounts are necessary in a resource group, storageAccountName variable refers to an already created storage account in this resource group
    * Microsoft.Network/publicIPAddresses 
    * Microsoft.Network/virtualNetworks
    * Microsoft.Network/networkInterfaces
    * Microsoft.Compute/virtualMachines
    * Microsoft.KeyVault/vaults/accessPolicies
    * Microsoft.Compute/virtualMachines/extensions

There is one json which includes all components: creating Key Vault, creating a Service Bus Queue and creating a VM with MSI enabled and access to a Key Vault. These components are also seperated into 3 seperate files each for one of the 3 components, so that it is easier to use for individual use cases. 

The final output should be the connection string followed by "message is sent to queue" and the next line would output the message id and message body that was received from the queue.

