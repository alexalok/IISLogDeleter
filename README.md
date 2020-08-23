# What is IISLogDeleter?

As the name implies, this is a simple tool that can be configured to delete all files from a specified folder with a given interval. While it was initially created to delete the old IIS logs it can be used in any similar scenario, such as deleting logs of some program.

# Installation

The program does not have an installation process. Simply clone the repo and build the project or download pre-built binaries from the [releases](https://github.com/alexalok/IISLogDeleter/releases/latest).

# Configuration 

Configuration of this program is straightforward: open the `appsettings.json` file with your favorite editor and change the settings to your like:

* `DeletionInterval` -  specifies for how long the program should wait between iterations (i.e. subsequent log deletions);
* `DeleteFilesOlderThan` - specifies for how long the log file must have been untouched to be deleted during an iteration. More specifically, **last write** time is checked;
* `LogsFolder` - folder on which to operate on (recursive).

# Running as a standalone

The program can be started in a standalone mode by simply executing `IISLogDeleter.exe` file. Settings from `appsettings.json` are automatically loaded.

# Running as a Windows Service

IISLogDeleter has a built-in ability to run as a Windows Service, which should be preferred over a standalone solution. To do that, copy the program files to the target server, then create a service. To do that, you can open PowerShell as an Administrator and type the following command:

`New-Service -Name "IISLogDeleter" -BinaryPathName "C:\IISLogDeleter\IISLogDeleter.exe" -StartupType AutomaticDelayedStart`

*(don't forget to change the path in the example!)*

More information on the `New-Service` cmdlet can be found [here](https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.management/new-service?view=powershell-7).

After creation, the service must be started **either** by:

* restarting a PC (if startup type used is `AutomaticDelayedStart`);
* opening `Services.msc` and starting the service manually;
* executing the cmdlet `Start-Service IISLogDeleter`