# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["Porpoise.Core.sln", "./"]
COPY ["Porpoise.Api/Porpoise.Api.csproj", "Porpoise.Api/"]
COPY ["Porpoise.Core/Porpoise.Core.csproj", "Porpoise.Core/"]
COPY ["Porpoise.DataAccess/Porpoise.DataAccess.csproj", "Porpoise.DataAccess/"]

# Restore dependencies
RUN dotnet restore "Porpoise.Core.sln"

# Copy everything else
COPY . .

# Build and publish
WORKDIR "/src/Porpoise.Api"
RUN dotnet publish "Porpoise.Api.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:$PORT
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Porpoise.Api.dll"]
