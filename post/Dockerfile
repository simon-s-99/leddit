# Set the base image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

# Set the working directory
WORKDIR /app

# Expose the API port
EXPOSE 80

# Copy post csproj and ledditmodels csproj and restore as distinct layers
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