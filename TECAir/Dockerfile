# 1. COMPILATION STAGE (SDK)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files (.csproj)
COPY ["TECAir.API/TECAir.API.csproj", "TECAir.API/"]
COPY ["TECAir.Core/TECAir.Core.csproj", "TECAir.Core/"]
COPY ["TECAir.Data/TECAir.Data.csproj", "TECAir.Data/"]

# Restore dependencies
RUN dotnet restore "TECAir.API/TECAir.API.csproj"

# Copy the rest of the code and compile
COPY . .
WORKDIR "/src/TECAir.API"
RUN dotnet build "TECAir.API.csproj" -c Release -o /app/build

# 2. PUBLISH STAGE
FROM build AS publish
RUN dotnet publish "TECAir.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 3. FINAL STAGE
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "TECAir.API.dll"]
