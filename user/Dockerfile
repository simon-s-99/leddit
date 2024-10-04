# Use the .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy both csproj files for dependency resolution
COPY . ./
COPY ["user/userIdentityAPI.csproj", "user/"]
COPY ["LedditModels/LedditModels.csproj", "LedditModels/"]

# Clean obj and bin folders to avoid conflicts
#RUN rm -rf /src/user/obj /src/user/bin
#RUN rm -rf /src/LedditModels/obj /src/LedditModels/bin

# Restore dependencies for both projects
RUN dotnet restore "user/userIdentityAPI.csproj"
RUN dotnet restore "LedditModels/LedditModels.csproj"


# Build the userIdentityAPI project
#RUN dotnet build "userIdentityAPI.csproj" -c Release -o /app/build

# Publish the app
FROM build-env AS publish
RUN dotnet publish "user/userIdentityAPI.csproj" -c Release -o /app/out

# Set up the runtime image
# Use the ASP.NET Core runtime image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 8080
EXPOSE 8081
COPY --from=publish /app/out .

#WORKDIR /app/out
ENTRYPOINT ["dotnet", "userIdentityAPI.dll"]