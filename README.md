# Perfect-Scheduler
Windows service using Asp.Net core to create zip file and upload to the Azure blob storage daily, weekly, monthly using Quartz with Dependency Injection, NLog

Windows services are a good way for reducing some manual jobs that we have to do in our system. 

The jobs of this windows service is given below.

- Zip the folder and save the file to a particular directory
- Upload the zipped folder to the Azure blob storage
- You also have an option to add any database so that you can save the scheduler information and make it persistent

Daily, Weekly, Monthly. Please feel free to read this article here: https://sibeeshpassion.com/tag/quartz-scheduler/
