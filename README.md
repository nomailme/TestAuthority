# TestAuthority

[![Build status](https://ci.appveyor.com/api/projects/status/9xmg595d0ps2r0uw?svg=true)](https://ci.appveyor.com/project/nomailme/testauthority)

Provides an easy way to issue SSL certificate for a specific host.

# Usage

Issue certificate for example.com

`http://localhost:5000/api/certificate?hostname=example.com&ipaddress=10.10.1.10`

Get root certificate

`http://localhost:5000/api/certificate/root`

# Swagger enabled

You can use swagger for simple and explicit certificate issue.

`http://localhost:5000/swagger`
