# Open Issues

This document highlights some technical issues that may need further actions.  
In places we may add Microsoft GitHub issues and in others this repo may need to change.

## 1. Layered Provisioning

The deployment uses [layered provisioning](https://devblogs.microsoft.com/azure-sdk/azure-developer-cli-azd-november-2025/), which is a beta feature since November 2025:

- [Azure Developer CLI Tutorial](https://devblogs.microsoft.com/devops/azure-developer-cli-azure-container-apps-dev-to-prod-deployment-with-layered-infrastructure/)
- [Azure Developer CLI Example](https://github.com/puicchan/azd-dev-prod-aca-storage)

Layered provisioning deploys supporting infrastructure first.  
The `azure.yaml` services then showcase applications, developer experience and business value.

### 1.1. azd up

Create a new environment on a local computer, then run `azd up` for the first time.  
The `preprovision.sh` hook gets called during `base` provisioning but not during `identity` provisioning.  

Also, the user gets prompted for technical parameters like `CONTAINER_APPS_ENVIRONMENT_ID`.  
These values do not seem to get picked up from [infra/identity/main.parameters.json](infra/identity/main.parameters.json).  

If you quit the deployment and re-run `azd up` again, everything works OK, so there is an easy workaround.

### 1.2. azd pipeline config

Similarly, when `azd pipeline config` is run, the user is prompted for parameters that the `base` layer outputs.  
Again, the user is prompted for values like `CONTAINER_APPS_ENVIRONMENT_ID`, which does not exist yet.  
We can work around this by temporarily commenting out the identity layer in `azure.yaml`.

```yaml
infra:
  layers:
    - name: base
      path: ./infra/base
    #- name: identity
    #  path: ./infra/identity
```

## 2. Microsoft Conformance

This GitHub repository strives to follow best practices and pass Microsoft automated checks.  
GitHub workflows can run the [template-validation-action](https://github.com/microsoft/template-validation-action) to validate the repository.

### 2.1. Basic Validation

With the following options, READMEs and bicep are validated to get a `CONFORMING` result:

```yaml
- uses: microsoft/template-validation-action@Latest
  id: validation
  with:
    useDevContainer: false
    validateAzd: false
```

### 2.2. Dynamically Created Files

The infrastructure layer uses three dynamically created files, created in a `preprovision.sh` script:

- External API gateway routes that use the deployment's external domain name.
- Internal API gateway routes that use the deployment's environment name.
- A cluster configuration file for the Curity Identity Server.

The validation requires `loadTextContent` paths in [infra/identity/main.bicep](infra/identity/main.bicep) to exist before deployment begins.  
This seems to be a [azd current limitation](https://github.com/Azure/bicep/issues/3816), which we work around by checking in files with dummy content.  

### 2.3. Automated Deployment and Teardown

With the following options, the validation runs deployment with `azd up` and then undoes it with `azd down`.  

```yaml
- uses: microsoft/template-validation-action@Latest
  id: validation
  with:
    useDevContainer: false
    validateAzd: true
```

The automated deployment succeeds, but only deploys the base provisioning layer and services.  
For the identity layer, it is unclear how the validator should handle [manual prerequisite actions](docs/GITHUB-WORKFLOW.md):

- Configure GitHub secrets that the deployment needs.
- Grant the GitHub workflow permissions to create an Entra ID app registration.

### 2.4. Dev Containers

We did not feel that a dev container environment would provide value for our [use case](.devcontainer/README.md).  
If this is a problem we can add artifacts, but the dev container may not enable an end-to-end flow.

## 3. Alternative Deployments

If reviewers consider layered provisioning issues to be blocking issue, we could remove it.  
The identity components would then be deployed as services instead of a provisioning layer.
That might introduce its own issues though, like having to output the SQL admin password during provisioning.
