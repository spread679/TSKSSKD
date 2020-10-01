# TSKSSKD - TaSK Service SKeDuler

A scheduler service to run batch and executable file.

I want to improve this service:

* create a functionality to remove log files;
* create the possibility to generate specif file batch (for example generate a batch to made a scheduler backup);
* create a details file to interact with the service;

Actualy the service have two folder:

* the LOG/ folder to store the log files;
* the TSK/ folder to enter the differents files with the differents execution;
***

## TSK Folder

The extension file to insert in this folder is **.tsk.txt**.
In these files, to start a service you need to enter a batch or executable file.
The syntax is : [minute]	[hour]	[day]	[month]	[week]	[file to execute]
For example: `1	*	*	*	*	C:\path\executable\file.exe`
Remember to use tab between the different specification.

In the *week field* you can enter the week in these format: *Sun-Mon-Tue-Wed-Thu-Fri-Sat* or *0-1-2-3-4-5-6*
If you use the week format the file will be execute a specific time.
***

## LOG Folder

The service create differents log files.
