# C#. UAC ByPass

The project goal is to study how UAC ByPass works and the techniques involved.

The code is based on [rootm0s](https://github.com/rootm0s) python implementation. If you want to know more about his job and the methods he used, you can find his GitHub in here: [https://github.com/rootm0s/WinPwnage](https://github.com/rootm0s/WinPwnage)

# How to use it?

I'm not providing the .exe, you need to compile it. Each file includes the *how to*. Basically, you need to have csc installed (usually comes with Visual Studio), open the Developer cmd and type:

Compile:	`csc.exe uac_bypass_xx.cs`

Usage:		`uac_bypass_xx.exe C:\Path\To\Payload.exe`

# Techniques implemented

*UAC Bypass fodhelper
*UAC Bypass computerdefaults
*UAC Bypass silentcleanup

# Video PoC

{% include video id="sY0xXTxNkqY" provider="youtube" %}

