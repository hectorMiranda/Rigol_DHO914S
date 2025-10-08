# Build the isolated Functions app and run it on the official Functions base image.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY src/RigolStream.Api/RigolStream.Api.csproj src/RigolStream.Api/
RUN dotnet restore src/RigolStream.Api/RigolStream.Api.csproj
COPY src/ src/
RUN dotnet publish src/RigolStream.Api/RigolStream.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated9.0 AS runtime
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
    Oscilloscope__Mode=Simulated
COPY --from=build /app/publish /home/site/wwwroot
