## [3.1.0]

### Changed

Added TopCount support in QueryByAttribute, and throw exception if both TopCount and PageInfo are set: DynamicsValue/fake-xrm-easy#16
Removed .netcoreapp3.1 target framework in versions 2.x, it'll be supported from versions 3.x onwards. Bump

## [3.0.2]

### Changed 

Bump Dataverse dependency to 0.6.1 from 0.5.10 to solve DynamicsValue/fake-xrm-easy#20
Also replaced Microsoft.Dynamics.Sdk.Messages dependency, as it has also been deprecated by MSFT, to Microsoft.PowerPlatform.Dataverse.Client.Dynamics 0.6.1 DynamicsValue/fake-xrm-easy#20

## [3.0.1-rc1] - Initial release

