FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TestAuthority.Model/TestAuthority.Model.csproj", "TestAuthority.Model/"]
RUN dotnet restore "TestAuthority.Model/TestAuthority.Model.csproj"
COPY . .
WORKDIR "/src/TestAuthority.Model"
RUN dotnet build "TestAuthority.Model.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TestAuthority.Model.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TestAuthority.Model.dll"]
