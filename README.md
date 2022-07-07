# TestAuthority

Provides an easy way to issue SSL certificate(PFX,PEM) for a specific host.
Contains tools for conversion to/from PEM format from/to PFX (PKCS12)

# Quickstart

## Requirements

* .NET 6 https://www.microsoft.com/net/download

To start Certificate Authority  
`dotnet TestAuthority.dll`

To start project in docker container

`docker run -p 5000:80 -d nomail/test-authority:latest`

`docker run -p 5000:80 -v /usr/share/test-authority:/usr/share/test-authority -d nomail/test-authority:latest`

# Usage

Issue certificate for example.com

`http://localhost:5000/api/certificate?commonName=test-certificate&hostname=example.com&ipaddress=10.10.1.10&format=pem`

Get root certificate

`http://localhost:5000/api/certificate/root`

Get dummy CRL file

`http://localhost:5000/api/crl`

# Docker

https://hub.docker.com/r/nomail/test-authority/

# Swagger enabled (WebUI)

You can use swagger UI for simple and explicit certificate issue.

`http://localhost:5000`
