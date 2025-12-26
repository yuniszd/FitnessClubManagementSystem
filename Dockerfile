# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything for building
COPY . .

# Restore the Web API project
RUN dotnet restore "src/Presentation/FCMS.WebApi/FCMS.WebApi.csproj"

# Build the project in Release mode
RUN dotnet build "src/Presentation/FCMS.WebApi/FCMS.WebApi.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "src/Presentation/FCMS.WebApi/FCMS.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage: copy the published app to the runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FCMS.WebApi.dll"]