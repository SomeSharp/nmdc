# SomeSharp.Nmdc

Simple .NET library to deal with the NeoModus Direct Connect (NMDC) protocol. Documentation on the latest version of the protocol can be found at [The NMDC Project](http://nmdc.sourceforge.net/Versions/NMDC-1.3.html).

**The library provides client-to-hub features only.**
The library exposes tools for working with client-to-hub connections only. No client-to-client features provided.

The library written in C# 6 (yes, it uses new features such as [interpolated strings](https://msdn.microsoft.com/en-us/library/dn961160.aspx) and [null-conditional operators](https://msdn.microsoft.com/en-us/library/dn986595.aspx)) and provides some basic classes representing NMDC entities, e.g. **NmdcHubConnection** and limited set of commands.

# Supported commands

At now the library supports not all NMDC commands. Since client-to-client connections are not supported, some commands will never be implemented in the library. List of implemented commands along with corresponding classes (in parentheses) is placed below:

* Chat command (**NmdcChatCommand**)
* $GetNickList (**GetNickListCommand**)
* $Hello (**NmdcHelloCommand**)
* $HubName (**NmdcHubNameCommand**)
* $Key (**NmdcKeyCommand**)
* $Lock (**NmdcLockCommand**)
* $MyInfo (**NmdcMyInfoCommand**)
* $NickList (**NmdcNickListCommand**)
* $OpList (**NmdcOpListCommand**)
* $Quit (**NmdcQuitCommand**)
* $Supports (**NmdcSupportsCommand**)
* $To (**NmdcToCommand**)
* $UserCommand (**NmdcUserCommandCommand**)
* $ValidateNick (**NmdcValidateNickCommand**)
* $Version (**NmdcVersionCommand**)
* $BadPass (**NmdcBadPassCommand**)
* $GetPass (**NmdcGetPassCommand**)
* $MyPass (**NmdcMyPassCommand**)

All other commands coming in via an instance of the **NmdcHubConnection** class will be recognized as **NmdcUnknownCommand**.
