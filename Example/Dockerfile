ARG RUNTIME_REPO=mcr.microsoft.com/dotnet/aspnet:7.0
ARG SDK_REPO=mcr.microsoft.com/dotnet/sdk:7.0

# Build C# project
FROM $SDK_REPO AS build
RUN echo | uname -a
WORKDIR /src
COPY . ./

WORKDIR "/src"
RUN dotnet build "Example.csproj" -c Release -o /app

# Publish C# project
FROM build AS publish
RUN dotnet publish -c Release -o /app
COPY secrets /app/secrets

FROM $RUNTIME_REPO AS base
ARG HTTP_PORT
ARG HTTPS_PORT
EXPOSE  $HTTP_PORT
EXPOSE  $HTTPS_PORT
ENV ASPNETCORE_URLS="http://+:5000;https://+:5001"
VOLUME /secrets
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Example.dll"]