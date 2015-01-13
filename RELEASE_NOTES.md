### 1.0.3 - January 13 2015
* Fixed a bug that prevented the `IrcClient.MessageReceived` event from stopping
* Performance improvements for synchronous message reading methods on `IrcClient`

### 1.0.2 - January 07 2015
* Parameterless IRC commands no longer contain a trailing space in their string representation
* The `IrcClient.MessageReceived` event raises unhandled exceptions instead of stopping silently

### 1.0.0 - January 02 2015
* Initial release (beta)