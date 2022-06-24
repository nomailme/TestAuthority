FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /src
COPY ["source/TestAuthority.Host/TestAuthority.Host.csproj", "TestAuthority.Host/"]
COPY ["source/TestAuthority.Application/TestAuthority.Application.csproj", "TestAuthority.Application/"]
COPY ["source/TestAuthority.Domain/TestAuthority.Domain.csproj", "TestAuthority.Domain/"]
RUN dotnet restore "/src/TestAuthority.Host/TestAuthority.Host.csproj"
COPY . .
RUN ls -al
RUN pwd
WORKDIR "/src/source/TestAuthority.Host"
RUN ls -al
RUN dotnet build "TestAuthority.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TestAuthority.Host.csproj" -c Release -o /app/publish

FROM base AS final
MAINTAINER Iskanders Jarmuhametovs (nomail86<-at->gmail.com)

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TestAuthority.Host.dll"]