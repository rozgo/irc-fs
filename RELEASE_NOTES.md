### 1.0.7 - January 16 2015
* Added support for building under Mono
* Added .travis.yml

### 1.0.6 - January 15 2015
* Fixed `IrcClient.Dispose` throwing an exception
* Fixed exception messages for `IrcClient` methods that throw `ObjectDisposedException`

### 1.0.5 - January 14 2015
* Added appveyor.yml
* Normalized assembly and root namespace naming
* Parameterized build scripts

### 1.0.4 - January 14 2015
* Added the FAKE build system

### 1.0.3 - January 13 2015
* Fixed a bug that prevented the `IrcClient.MessageReceived` event from stopping
* Performance improvements for synchronous message reading methods on `IrcClient`

### 1.0.2 - January 07 2015
* Parameterless IRC commands no longer contain a trailing space in their string representation
* The `IrcClient.MessageReceived` event raises unhandled exceptions instead of stopping silently

### 1.0.0 - January 02 2015
* Initial release (beta)