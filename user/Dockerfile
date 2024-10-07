# Use the .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy both csproj files for dependency resolution
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src
COPY ["user/userIdentityAPI.csproj", "user/"]
COPY ["LedditModels/LedditModels.csproj", "LedditModels/"]
RUN dotnet restore "user/userIdentityAPI.csproj"
COPY . .

# Clean obj and bin folders to avoid conflicts
#RUN rm -rf /src/user/obj /src/user/bin
#RUN rm -rf /src/LedditModels/obj /src/LedditModels/bin

# Restore dependencies for both projects
#RUN dotnet restore "LedditModels/LedditModels.csproj"


# Build the userIdentityAPI project
WORKDIR /src/user
RUN dotnet build "userIdentityAPI.csproj" -c Release -o /app


# Publish the app
FROM build-env AS publish
WORKDIR /src/user
RUN dotnet publish "userIdentityAPI.csproj" -c Release -o /app

# Set up the runtime image
# Use the ASP.NET Core runtime image for running the app
FROM base AS final
COPY --from=publish /app .

WORKDIR /app
ENTRYPOINT ["dotnet", "userIdentityAPI.dll"]