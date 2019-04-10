# Powershell script to deploy ARM Template to Azure.
[Cmdletbinding()]
param ( 
    [parameter(Mandatory = $false)]
    [string]$subscriptionName = "Visual Studio Enterprise",

    [parameter(Mandatory = $false)]
    [string]$resourceGroupName = "HelloARM"
)

begin {
    Login-AzureRmAccount
    Try {
        Select-AzureRmSubscription -SubscriptionName $subscriptionName -ErrorAction Stop -Verbose| Out-Null
    }
    catch {
        Write-Host -ForegroundColor Red "Subscription Name is not valid" 
        break;
    }
}
process {  
    #New-AzureRmResourceGroup -Name $resourceGroupName -Location "East Us" -Verbose -Force
    New-AzureRmResourceGroupDeployment -ResourceGroupName $resourceGroupName -TemplateFile "$PSScriptRoot\azuredeploy.json" -TemplateParameterFile "$PSScriptRoot\params\azuredeploy.parameters.json"    

}
