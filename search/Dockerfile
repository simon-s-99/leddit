# Set .net 8.0 as base image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

WORKDIR /app

# Expose the port the API will listen on
EXPOSE 9200

COPY . ./
COPY ["search/", "Search/"]
COPY ["LedditModels/LedditModels.csproj", "LedditModels/"]

# Dockerfile does not find Main() during build
# if this vVv line is not present 
#COPY ["search/Program.cs", "Search/Program.cs"]

RUN dotnet restore "Search/Search.csproj"
#RUN dotnet restore "LedditModels/LedditModels.csproj"

FROM build-env AS publish
WORKDIR /app/Search
RUN dotnet publish "Search.csproj" -c Release -o /app/out

# Build the runtime image
#FROM mcr.microsoft.com/dotnet/aspnet:8.0
#FROM build-env AS final
#WORKDIR /app
#COPY --from=build-env /app/out .

ENTRYPOINT ["dotnet", "Search.dll"]
