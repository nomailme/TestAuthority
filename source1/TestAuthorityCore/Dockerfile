FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 5000
                                                              
COPY /out .
ENTRYPOINT ["dotnet", "TestAuthorityCore.dll"]
