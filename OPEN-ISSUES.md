# Open Issues

This document highlights some minor `azd` technical issues and also some conformance details.  

## 1. Layered Provisioning

The deployment uses [layered provisioning](https://devblogs.microsoft.com/azure-sdk/azure-developer-cli-azd-november-2025/), which is a beta feature since November 2025:

- [Azure Developer CLI Tutorial](https://devblogs.microsoft.com/devops/azure-developer-cli-azure-container-apps-dev-to-prod-deployment-with-layered-infrastructure/)
- [Azure Developer CLI Example](https://github.com/puicchan/azd-dev-prod-aca-storage)

Layered provisioning deploys supporting infrastructure before applications.  
The `azure.yaml` services then showcase applications, developer experience and business value.

### 1.1. Layered Provisioning Minor Issues

A couple of GitHub issues were raised related to layered provisioning.  
One of these leads to an `azd up` bug the first time you run it, which you can resolve with a retry.

- [Resolve dependencies between provisioning layers before prompting](https://github.com/Azure/azure-dev/issues/7182)
- [Support hooks per provisioning layer](https://github.com/Azure/azure-dev/issues/7186)

## 2. Key Vault

Local deployments to Azure write secrets to an Azure key vault.  
Doing so enables `azd pipeline config` to automatically transfer secrets to GitHub workflows later.

### 2.1. Setting Secrets in Hooks

The deployment uses hooks to generate strong backend secrets and I would like to use the following command:

```bash
azd env set-secret MYSECRET 'my value'
```

The command writes a value of the following form to the `.env` file, to support [transfer to GitHub](https://github.com/Azure/azure-dev/blob/main/cli/azd/docs/using-environment-secrets.md#secrets):

```bash
MYSECRET="akvs://3d52ec16-06b8-4b44-bdfd-9fdd056e16f1/kv-devvnfh4isv54wpu/MYSECRET"
```

If I try to call `azd env set-secret` silently, it cannot work out the key vault name.  
I work around this by calling the `az` command and updating the `.env` file manually.

## 3. Microsoft Conformance

This GitHub repository strives to follow best practices and pass Microsoft automated checks.  
GitHub workflows can run the [template-validation-action](https://github.com/microsoft/template-validation-action) to validate the repository.

### 3.1. Basic Validation

With the following options, READMEs and bicep are validated to get a `CONFORMING` result.  

```yaml
- uses: microsoft/template-validation-action@Latest
  id: validation
  with:
    useDevContainer: false
    validateAzd: false
```

### 3.2. Automated Deployment and Teardown

With the following options, the validation runs deployment with `azd up` and then undoes it with `azd down`.  
The automated deployment succeeds, but seems to not deploy the identity provisioning layer.

```yaml
- uses: microsoft/template-validation-action@Latest
  id: validation
  with:
    useDevContainer: false
    validateAzd: true
```

### 3.3. Dynamically Created Files

The infrastructure layer uses three dynamically created files, created in a `preprovision.sh` script:

- External API gateway routes that use the deployment's external domain name.
- Internal API gateway routes that use the deployment's environment name.
- A cluster configuration file for the Curity Identity Server.

The validation requires `loadTextContent` paths in [infra/identity/main.bicep](infra/identity/main.bicep) to exist before deployment begins.  
This seems to be a [azd current limitation](https://github.com/Azure/bicep/issues/3816), which we work around by checking in files with dummy content.  
To prevent checkins of the dummy files, you can run the following script:

```bash
./tools/utils/prevent-dynamic-file-checkins.sh
```

### 3.4. Dev Containers

We did not feel that a dev container environment would provide value for our [use case](.devcontainer/README.md).  
If this is a problem we can add artifacts, but the dev container may not enable an end-to-end flow.
