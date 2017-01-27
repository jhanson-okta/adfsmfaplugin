Okta MFA Adapter for ADFS 2012 R2
---------------------------------
Install on Each Server in the ADFS Farm using the Powershell script attached.


Enables Okta Multi-Factor Authentication to be completed from a Microsoft ADFS Server.
Current functionality includes text message verification and Okta Verify (both Push and Code)

This code is not supported by Okta or its affiliates and has been created in order to 
demonstrate the flexibility and extensibility of Okta's API surface for MFA.

To deloy plugin, download and extract okta.zip to the c drive, update "OktaMFA-ADFS.dll.config" with your tenant name and API key and run the powershell commands as an administrator. If you make updates, copy and paste the new dlls to the c:\Okta and re-run the powershel commands. I recommend just copying the files to Powershell ISE.

Powershell.

Unregister-AdfsAuthenticationProvider -Name "OktaMFASMS-ADFS"
Unregister-AdfsAuthenticationProvider -Name "OktaMFA-ADFS"
Unregister-AdfsAuthenticationProvider -Name "OktaMFAPush"
Restart-Service adfssrv -Force
Stop-Service adfssrv -Force
$path = "C:\Okta"
Copy-Item "$path\OktaMFA-ADFS.dll.config" -Destination "C:\Windows\ADFS\OktaMFA-ADFS.dll.config" -Force
Set-Location $path
[System.Reflection.Assembly]::Load("System.EnterpriseServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")
$newton = New-Object System.EnterpriseServices.Internal.Publish
$newton.GacInstall($path + "\SMS\Newtonsoft.Json.dll")
$newton2 = New-Object System.EnterpriseServices.Internal.Publish
$newton2.GacInstall($path + "\Verify\Newtonsoft.Json.dll")

$publish = New-Object System.EnterpriseServices.Internal.Publish
$publish.GacInstall($path + "\Verify\OktaMFAPush.dll")
$publish1 = New-Object System.EnterpriseServices.Internal.Publish
$publish1.GacInstall($path + "\SMS\OktaMFASMS-ADFS.dll")
$typeName = "OktaMFASMS_ADFS.AuthenticationAdapter, OktaMFASMS-ADFS, Version=1.0.0.0, Culture=neutral, PublicKeyToken=0fef15342f59ac09"
$typeName1 = "OktaMFAPush.AuthenticationAdapter, OktaMFAPush, Version=1.0.0.0, Culture=neutral, PublicKeyToken=0fef15342f59ac09"
Start-Service adfssrv
Register-AdfsAuthenticationProvider -TypeName $typeName -Name "OktaMFASMS-ADFS" -Verbose
Register-AdfsAuthenticationProvider -TypeName $typeName1 -Name "OktaMFAPush" -Verbose
Restart-Service adfssrv -Force
