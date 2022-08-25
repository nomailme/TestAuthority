FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base

WORKDIR /app


FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /src
COPY ["source/TestAuthority.Host/TestAuthority.Host.csproj", "TestAuthority.Host/"]
COPY ["source/TestAuthority.Application/TestAuthority.Application.csproj", "TestAuthority.Application/"]
COPY ["source/TestAuthority.Domain/TestAuthority.Domain.csproj", "TestAuthority.Domain/"]
COPY ["source/TestAuthority.Infrastructure/TestAuthority.Infrastructure.csproj", "TestAuthority.Infrastructure/"]
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

EXPOSE 80

WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir -p /usr/share/test-authority
ENTRYPOINT ["dotnet", "TestAuthority.Host.dll"]