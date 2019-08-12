# Perfect-Scheduler
Windows service using Asp.Net core to create zip file and upload to the Azure blob storage daily, weekly, monthly using Quartz with Dependency Injection, NLog

Windows services are a good way for reducing some manual jobs that we have to do in our system. 

The jobs of this windows service is given below.

- Zip the folder and save the file to a particular directory
- Upload the zipped folder to the Azure blob storage

Daily, Weekly, Monthly. Please feel free to read this article here: https://sibeeshpassion.com/asp-net-core-windows-service-task-scheduler-daily-weekly-monthly/.
