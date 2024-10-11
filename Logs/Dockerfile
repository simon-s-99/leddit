# Set the base image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

# Set the working directory
WORKDIR /app

# Expose the API port
EXPOSE 80

# Copy csproj and restore as distinct layers
#COPY *.csproj ./
COPY ["Logs/Logs.csproj", "Logs/"]
COPY ["LedditModels/LedditModels.csproj", "LedditModels/"]
# Copy everything else and build the API
COPY . ./
WORKDIR "/app/Logs"
RUN dotnet restore "Logs.csproj"

RUN dotnet publish "Logs.csproj" -c Release -o /app/out

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

# Set the entry point for the API
ENTRYPOINT ["dotnet", "Logs.dll"]