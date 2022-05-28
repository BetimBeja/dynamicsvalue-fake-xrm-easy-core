## [Unreleased]

### Changed

- Modified TopCount support in QueryByAttribute and QueryExpression, to not throw exception if PageInfo was set but empty: DynamicsValue/fake-xrm-easy#16
- Do not clear previous FakeMessageExecutors or GenericFakeMessageExecutors when adding new ones or when calling them multiple times: DynamicsValue/fake-xrm-easy#15

## [2.1.0]

### Changed

Added TopCount support in QueryByAttribute, and throw exception if both TopCount and PageInfo are set: DynamicsValue/fake-xrm-easy#16
Removed .netcoreapp3.1 target framework in versions 2.x, it'll be supported from versions 3.x onwards.
Bump Microsoft.CrmSdk.CoreAssemblies to version 9.0.2.27 to support plugin telemetry - DynamicsValue/fake-xrm-easy#24

## [2.0.1-rc1] - Initial release