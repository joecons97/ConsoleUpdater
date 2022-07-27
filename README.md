# ConsoleUpdater
A simple .Net Console App auto updater

To begin:
```
Add Reference to ConsoleUpdater
```

Create ConsoleUpdater Object
```
var updater = ConsoleUpdater.Create().
                WithDomain("http://www.yourserver.com/").
                WithVersionFile("versiontracker.txt").
                WithAppFile("yourpacket.zip");
```

Version Tracker required format:
```
1.0.0.0
```
Update as required

The system compares the update to the version of the entry assembly's (your console app) version

Run Update Check
```
if (updater.CheckForUpdate(out var v))
  Console.Read();
else
  Console.WriteLine($"No update ({v})");
```
