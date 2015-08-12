# Simple test #1
A very simple test example is supplied that demonstrates the round trip operation of nhooked.

To execute:
* Open and build the solution
* Open a command prompt, and navigate to (under your clone) nhooked\src\Hooked\bin\Debug
* Type Hooked <ENTER>
* The simple nhooked example is now running, awaiting a message to process

For my testing, I then open the advanced REST client in Chrome (see the Chrome store), and enter:
* URL: http://localhost:55555
* Http verb: POST
* A raw payload of:
{ 
 "TopicName": "Test",
 "ChannelMonicker": "Client",
 "Sequence" : "0",
 "WhenCreated": "2015-08-11 09:34:01",
 "Body": { 
     "Id": 72,
     "Action": "Update",
     "Success": true
  }
}
* Optionally change the content type of the request
* Press Send

All being well, the console window should show a handler being activated, and a message dumped as well.