# TestAuthority

[![Build status](https://ci.appveyor.com/api/projects/status/9xmg595d0ps2r0uw?svg=true)](https://ci.appveyor.com/project/nomailme/testauthority)

Provides an easy way to issue SSL certificate for a specific host.

# Quickstart

## Requirements

* .NET Core 2.1 https://www.microsoft.com/net/download

To start Certificate Authority  
`dotnet TestAuthority.dll`

# Usage

Issue certificate for example.com

`http://localhost:5000/api/certificate?hostname=example.com&ipaddress=10.10.1.10`

Get root certificate

`http://localhost:5000/api/certificate/root`

# Docker

https://hub.docker.com/nomail/test-authority/

# Swagger enabled (WebUI)

You can use swagger for simple and explicit certificate issue.

`http://localhost:5000/swagger`

# Additional info

Docker file included but is not complete at the moment
