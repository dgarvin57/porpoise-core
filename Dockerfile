# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files for restore
COPY ["Porpoise.Api/Porpoise.Api.csproj", "Porpoise.Api/"]
COPY ["Porpoise.Core/Porpoise.Core.csproj", "Porpoise.Core/"]
COPY ["Porpoise.DataAccess/Porpoise.DataAccess.csproj", "Porpoise.DataAccess/"]

# Restore dependencies for API project only
WORKDIR "/src/Porpoise.Api"
RUN dotnet restore "Porpoise.Api.csproj"

# Copy everything else
WORKDIR /src
COPY . .

# Build and publish
WORKDIR "/src/Porpoise.Api"
RUN dotnet publish "Porpoise.Api.csproj" -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:$PORT
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Porpoise.Api.dll"]
