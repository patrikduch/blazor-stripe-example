# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project and restore
COPY ["BlazorStripeExampleApp/BlazorStripeExample.csproj", "BlazorStripeExampleApp/"]
RUN dotnet restore "./BlazorStripeExampleApp/BlazorStripeExample.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/BlazorStripeExampleApp"
RUN dotnet build "./BlazorStripeExample.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BlazorStripeExample.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# ✅ Set port to 80 and expose it
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

# Copy published app
COPY --from=publish /app/publish .

# ✅ Use correct DLL name
ENTRYPOINT ["dotnet", "BlazorStripeExample.dll"]
