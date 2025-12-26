# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all files to build the project
COPY . .

# Restore specifically for the WebApi project
RUN dotnet restore "src/Presentation/FCMS.WebApi/FCMS.WebApi.csproj"

# Build the WebApi project
RUN dotnet build "src/Presentation/FCMS.WebApi/FCMS.WebApi.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "src/Presentation/FCMS.WebApi/FCMS.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FCMS.WebApi.dll"]