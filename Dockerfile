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

EXPOSE 8080

COPY --from=publish /app/publish .

# Uses Heroku's PORT when present and port 8080 in local containers.
CMD ["sh", "-c", "dotnet Otw.Puzzli.Api.Authentication.dll --urls http://0.0.0.0:${PORT:-8080}"]
