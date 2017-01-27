$path = "C:\Okta"
Stop-Service adfssrv -Force
$newton = New-Object System.EnterpriseServices.Internal.Publish
$newton.GacInstall($path + "\Verify\Newtonsoft.Json.dll")