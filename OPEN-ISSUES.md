# Open Issues

This document highlights some minor azd technical issues and also some conformance details.

## 1. Layered Provisioning

The deployment uses [layered provisioning](https://devblogs.microsoft.com/azure-sdk/azure-developer-cli-azd-november-2025/), which is a beta feature since November 2025:

- [Azure Developer CLI Tutorial](https://devblogs.microsoft.com/devops/azure-developer-cli-azure-container-apps-dev-to-prod-deployment-with-layered-infrastructure/)
- [Azure Developer CLI Example](https://github.com/puicchan/azd-dev-prod-aca-storage)

Layered provisioning deploys supporting infrastructure before applications.  
The `azure.yaml` services then showcase applications, developer experience and business value.

### 1.1. Layered Provisioning Minor Issues

A couple of GitHub issues were raised related to layered provisioning:

- [Resolve dependencies between provisioning layers before prompting](https://github.com/Azure/azure-dev/issues/7182)
- [Support hooks per provisioning layer](https://github.com/Azure/azure-dev/issues/7186)

## 2. Microsoft Conformance

This GitHub repository strives to follow best practices and pass Microsoft automated checks.  
GitHub workflows can run the [template-validation-action](https://github.com/microsoft/template-validation-action) to validate the repository.

### 2.1. Basic Validation

With the following options, READMEs and bicep are validated to get a `CONFORMING` result.  

```yaml
- uses: microsoft/template-validation-action@Latest
  id: validation
  with:
    useDevContainer: false
    validateAzd: false
```

### 2.2. Automated Deployment and Teardown

With the following options, the validation runs deployment with `azd up` and then undoes it with `azd down`.  
The automated deployment succeeds, but seems to not deploy the identity provisioning layer.

```yaml
- uses: microsoft/template-validation-action@Latest
  id: validation
  with:
    useDevContainer: false
    validateAzd: true
```

### 2.3. Dynamically Created Files

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

### 2.4. Dev Containers

We did not feel that a dev container environment would provide value for our [use case](.devcontainer/README.md).  
If this is a problem we can add artifacts, but the dev container may not enable an end-to-end flow.
