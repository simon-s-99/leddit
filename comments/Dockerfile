# Set the base image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

# Set the working directory
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build the API
COPY . ./
RUN dotnet publish -c Release -o out

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

# Expose the API port
EXPOSE 80

# Set the entry point for the API
ENTRYPOINT ["dotnet", "Comments.dll"]