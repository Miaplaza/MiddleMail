# https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/building-net-docker-images?view=aspnetcore-3.1

#
# Dockerfile for MiddleMail.Server
#

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

COPY src/. ./src/

WORKDIR /app/src/

# restore in distinct layer
RUN dotnet restore MiddleMail.Server/MiddleMail.Server.csproj

RUN dotnet publish MiddleMail.Server/MiddleMail.Server.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6. AS runtime
WORKDIR /app
COPY --from=build /app/src/out ./
ENTRYPOINT ["dotnet", "MiddleMail.Server.dll"]
