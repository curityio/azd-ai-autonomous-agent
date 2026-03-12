# Open Issues

## Layered Provisioning

The deployment uses [layered provisioning](https://devblogs.microsoft.com/azure-sdk/azure-developer-cli-azd-november-2025/), which is a beta feature since November 2025:

- [Azure Developer CLI Article](https://devblogs.microsoft.com/devops/azure-developer-cli-azure-container-apps-dev-to-prod-deployment-with-layered-infrastructure/)
- [GitHub Example Repository](https://github.com/puicchan/azd-dev-prod-aca-storage)

## azd up

The first time you run `azd up` on a local computer, the `preprovision.sh` hook does not get called correctly.  
It only gets called for the base layer, not the identity layer, leading to unnecessary user prompts.  
E.g. for `CONTAINER_APPS_ENVIRONMENT_ID` expressed in [infra/identity/main.parameters.json](infra/identity/main.parameters.json).  

## azd pipeline config

Similarly, when `azd pipeline config` is run, the user is prompted for parameters output from the `base` layer.  
Again, the user is prompted for values like `CONTAINER_APPS_ENVIRONMENT_ID`.  
We can work around this by temporarily commenting out the identity layer in `azure.yaml`.

```yaml
infra:
  layers:
    - name: base
      path: ./infra/base
    #- name: identity
    #  path: ./infra/identity
```
