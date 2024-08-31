# DeltaLogger

DeltaLogger is a keylogger that uses TCP to connect from server to client over which keylogs are send based on what the user has specified in the build options. The logs are then stored locally and the IP addresses are added to a listbox after the server has started. The user can then select a specific IP to view the logs from.

## Building
* Open the project in Visual Studio
* Build the Delta project
* Build the DeltaLogger project

## Usage

### Building the client
* First run the DeltaLogger executable
* Choose the IP address. This is usually your own one which is set by default.
* Choose the port to run the server on
* Choose the interval (this is the time in miliseconds everytime the logs are sent back to the server)
* Click the browse button next to the input file
* Go to the directory where you Delta.exe executable was build and select it
* Click browse next to the output file and select a path to save the client
* Press Build

### Using the server
* Once the client is ran you can start the server
* Go to the Logs tab and enter the same IP address as you did before
* Enter the same port as you did before
* Click the Start Server button
* Now whenever the user is running the client and typing, eventually depending on the interval you initially entered the logs will be send to the log file
* Select the IP you want to view the logs for.
* The logs will then be automatically updated whenever new data is send.
