# AzureContainerInstancesLinux
Azure Container Instances demo using multiple Linux Containers

## What is this?

This project demonstrates how to use Azure Container Instances with multiple co-operating containers. 
It simulates a setup you could run in production.

When deployed, you'll have 3 containers running. 

1. jobgenerator
 An ASPNET core web api that is used to enqueue jobs on a Service bus queue.
2. jobprocessor
 An ASPNET core web api that is used to process jobs read from the Service bus queue.
3. logging
 An ASPNET core web api that is used to log informational messages from the other two containers. The information is logged into files, that are written to a mounted volume (an Azure File Share).

## Prerequisites

1. Create an Azure Service Bus namespace. 

  - Create a queue called 'TestQueue'.
  - Create two shared access policies: `Send` and `Listen`, with matching access rights.
  - Pass the connection strings of the policies to the deployment, for instance by storing them the file [azuredeploy.parameters.json](https://github.com/loekd/AzureContainerInstancesLinux/blob/master/azuredeploy.parameters.json)
     Remove the entitypath at the end, e.g.: `Endpoint=sb://my.servicebus.windows.net/;SharedAccessKeyName=Listen;SharedAccessKey=6BHFGHJUYTGHJYGO8ujk3ouyh+E=;`

2. Prepare a mountable File Share

  - Create an Azure Storage account. (general purpose)
  - Inside the storage account, create a file share called `acidemoshare`
  - Copy the account access key.
  - Paste the account key in the azuredeploy.parameters.json file under parameter `storageAccountKey`

After this, your `azuredeploy.parameters.json` file should look similar to this:

``` json
{
  "loggingContainerImage": {
    "value": "loekd/azurecontainerinstances.logging:4.0"
  },
  "jobGeneratorContainerImage": {
    "value": "loekd/azurecontainerinstances.jobgenerator:4.0"
  },
  "jobProcessingContainerImage": {
    "value": "loekd/azurecontainerinstances.jobprocessing:4.0"
  },
  "loggingServiceUrl": {
    "value": "http://localhost:8080"
  },
  "serviceBusConnectionStringSend": {
    "value": "Endpoint=sb://my.servicebus.windows.net/;SharedAccessKeyName=Send;SharedAccessKey=Nb5fgh+tyj46y4h35gheh==;"
  },
  "serviceBusConnectionStringListen": {
    "value": "Endpoint=sb://my.servicebus.windows.net/;SharedAccessKeyName=Listen;SharedAccessKey=dsfvsgsgb232gergEwe4gERBVERb+E=;"
  },
  "storageAccountName": {
    "value": "acidemo"
  },
  "storageAccountKey": {
    "value": "3BPpY82Qs1nYq9iD/45gethrh545/p1gerb43t3G3gtgDHE+hfreg34g4RGERG+afa3fGSD+dsvsgwDSF=="
  }
}
```



## Deploy the template     
     
1. Create a resource group in `westus`. *This won't work anywhere else (at the time of writing this).*

 ``` powershell
 az group create --name acidemo --location westus
 ```
 
2. Deploy the template to your resource group:

 ``` powershell
 az group deployment create --name ContainerGroup --resource-group acidemo --template-file azuredeploy.json --parameters azuredeploy.parameters.json`
```
 
3. Validate the deployment

  - Get the container group IP Address from the deployment output:

``` json
"outputs": {
   "containerIPv4Address": {
      "type": "String",
     "value": "1.2.3.4"
   }
},
```

  - In a browser, navigate to `http://1.2.3.4/api/jobs/ping` using the IP address from above as the host.
  - You should see the string `I am alive.` 
  
 ## Use it
 
 ### Enqueue some 'work'
 - In a browser, navigate to `http://1.2.3.4/api/jobs/?jobDescription=Perform Task 1` using the IP address from above as the host.
  This will enqueue a task called 'Perform Task 1'. The 'jobprocessor' container will receive and 'execute' the task. (simply by logging it)
  
 ### Validate processing
 - In a browser, navigate to `http://1.2.3.4/api/jobs/results` using the IP address from above as the host.
  This will query all logging from the 'logging' container and display it.
 - The output should be similar to this:
 
 ``` json
 [
    {
        "message": "JobGenerator - 00001 - 2017-08-16T08:20:17.5902050Z - Enqueueing job: Perform Task 1"
    },
    {
        "message": "JobProcessing - 00001 - 2017-08-16T08:20:21.3978540Z - Message processed: Perform Task 1."
    }
]
 ```
 
 The first message originated from 'jobgenerator', the second message originated from 'jobprocessing'.
 
 You now have your Azure Container Instances project running. 
 
 Feel free to use the ideas and source code in this project in any way you like.
 
 ## Debug
 Debugging should also work locally, provided you configure user-level environment variable **before you start Visual Studio**.
 `ServiceBusConnectionString` that has both Listen and Send rights on your Service Bus queue.

 When debugging, the Azure File Share is not used. Stuff is logged in memory, in a singleton object.
 
