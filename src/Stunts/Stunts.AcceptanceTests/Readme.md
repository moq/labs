# Moq.Proxy Acceptance Tests

The acceptance tests ensure that a broad range of types can 
be properly proxied by the proxy generator. 

In order to achieve this, all referenced assemblies in the 
project are enumerated, all public types are retrieved, and 
proxy generation is attempted on all of them. Failure to 
compile these generated proxies would indicate an unsupported 
scenario that would require fixing.

If you come across one such unsupported proxy generation 
scenario, please just add a nuget package reference to the 
project, run the tests (to cause the failure) and send a PR 
with the change so it can be fixed.