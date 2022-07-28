# ConsoleUpdater
A simple .Net Console App auto updater

To begin:
```
Add reference to ConsoleUpdater
```

Create ConsoleUpdater object:
```cs
var updater = ConsoleUpdater.Create().
    WithDomain("http://www.yourserver.com/").
    WithVersionFile("versiontracker.txt").
    WithAppFile("yourpackage.zip").
    WithProgressBarInfo(new ProgressBarInfo() { DisplayPercent = true, DisplaySpeed = true, CompleteColor = ConsoleColor.Yellow }). //See ProgressBarInfo for all options
    WithCustomVersion("1.0.0.0");  //Optional
```

Version-Tracker required format:
```
1.0.0.0
```
Update as required

The system compares the update to the version of the entry assembly's (your console app) version

Run Update Check
```cs
if (updater.CheckForUpdate(out var v))
  Console.Read();
else
  Console.WriteLine($"No update ({v})");
```
