# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["Otw.Puzzli.Api.Authentication.csproj", "./"]
RUN dotnet restore "Otw.Puzzli.Api.Authentication.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "Otw.Puzzli.Api.Authentication.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Otw.Puzzli.Api.Authentication.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Heroku uses PORT environment variable
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}

COPY --from=publish /app/publish .

# Use CMD instead of ENTRYPOINT for Heroku compatibility
CMD ASPNETCORE_URLS=http://*:$PORT dotnet Otw.Puzzli.Api.Authentication.dll
