# #See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
# USER app
# WORKDIR /app
# EXPOSE 8080

# FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# ARG BUILD_CONFIGURATION=Release
# WORKDIR /src
# COPY ["post.csproj", "."]
# RUN dotnet restore "./././post.csproj"
# COPY . .
# WORKDIR "/src/."
# RUN dotnet build "./post.csproj" -c $BUILD_CONFIGURATION -o /app/build

# FROM build AS publish
# ARG BUILD_CONFIGURATION=Release
# RUN dotnet publish "./post.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# FROM base AS final
# WORKDIR /app
# COPY --from=publish /app/publish .
# ENTRYPOINT ["dotnet", "post.dll"]

# Set the base image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

# Set the working directory
WORKDIR /app

# Expose the API port
EXPOSE 80

# Copy csproj and restore as distinct layers
#COPY *.csproj ./
COPY ["post/post.csproj", "post/"]
COPY ["LedditModels/LedditModels.csproj", "LedditModels/"]
# Copy everything else and build the API
COPY . ./
WORKDIR "/app/post"
RUN dotnet restore "post.csproj"

RUN dotnet publish "post.csproj" -c Release -o /app/out

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

# Set the entry point for the API
ENTRYPOINT ["dotnet", "post.dll"]