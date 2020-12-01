# CsLanBeacon
CsLanBeacon is a small UDP lan discovery library. It works by setting up a "Beacon", which is listening for a predefined key on a port, and a "Probe", that is broadcasting this key on the lan network. Multiple beacons can have the same key and respond simultaneously to a single, broadcasting Probe.
Creator: P H, ph1387@t-online.de 

---

## Overview
This repository contains three different projects:

1. The library itself
2. A Beacon console example application
3. A Probe console example application

Both console applications are examples of how to use the Beacon and Probe classes contained in the library itself. They work "out of the box" by simply compiling and running them. Since they are console applications there was no need foe using the UI thread Dispatcher in order to print text in the console. Should you use this library in i.e. a WPF application then all events that update the UI **must** be done by calling either one of the 
```cs
Dispatcher.Invoke(() => { ... });
Dispatcher.BeginInvoke((Action)(() => { ... }));
```
methods.

### Setting up a Beacon

```cs
var beacon = new Beacon("CustomKey", 5555);

beacon.BeaconActiveEvent += (s, e) => { Dispatcher.Invoke(() => { ... }); };
beacon.BeaconStoppedEvent += (s, e) => { ... };
beacon.BeaconResponseEvent += (s, e) => { ... };
beacon.Start();

// Do something. The beacon is listening in a background task.

beacon.Stop();
```

The 5555 is the port the beacon will be listening on. The default value for this is 8080 but it can be changed in the constructor or at runtime.

### Finding Beacons using a Probe

```cs
var probe = new Probe("CustomKey", TimeSpan.FromSeconds(1), 5555, 6666);

probe.ProbeActiveEvent += (s, e) => { Dispatcher.Invoke(() => { ... }); };
probe.ProbeStoppedEvent += (s, e) => { ... };
probe.ProbeBroadcastEvent += (s, e) => { . };
probe.ProbeReceivedResponseEvent += (s, e) => { ... };
probe.Start();

// Do something. The probe is sending UPD broadcast packets on the lan in a background task.

probe.Stop();
```

5555 is the port the UPD broadcast is send on. It must be the same as the targeted Beacon on the network. 8080 is the default value for this but it can be changed either in the constructor or at runtime.
6666 is the port the Probe is listening for answers. It can be set to the exact same value as the broadcasting port (here 5555). Doing so makes the Probe receive a valid answer from itself, therefore finding itself and it's own IPEndpoint on the lan. In general a different port should be used in order to only discover other hosts.

The "FindBeaconEndpointsAsync" function can be used to asynchronously discover a collection of responding Beacon IPEndpoints. The TimeSpan provided towards this function defines how long the Probe will be searching for Beacons on the network. When using this function make sure to provide a TimeSpan longer than the wait time between broadcasts. Not doing so will result in the Probe not finding any hosts at all!

```cs
public async void UpdateEndpointCollection()
{
    var probe = new Probe("CustomKey", TimeSpan.FromSeconds(1), 5555, 6666);
    var endpoints = await probe.FindBeaconEndpointsAsync(TimeSpan.FromSeconds(10));

    this.endpoints = endpoints;
}
```

## License
MIT [license](https://github.com/ph1387/CsLanBeacon/blob/master/LICENSE.txt)