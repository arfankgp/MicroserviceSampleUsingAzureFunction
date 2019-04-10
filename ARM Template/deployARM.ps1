# Powershell script to deploy ARM Template to Azure.
[Cmdletbinding()]
param ( 
    [parameter(Mandatory = $true)]
    [string]$subscriptionName,

    [parameter(Mandatory = $true)]
    [string]$resourceGroupLocation,

    [parameter(Mandatory = $true)]
    [string]$resourceGroupName
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
    New-AzureRmResourceGroup -Name $resourceGroupName -Location $resourceGroupLocation -Verbose -Force
    New-AzureRmResourceGroupDeployment -ResourceGroupName $resourceGroupName -TemplateFile "$PSScriptRoot\azuredeploy.json" -TemplateParameterFile "$PSScriptRoot\params\azuredeploy.parameters.json"    

}
