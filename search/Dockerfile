# set asp.net 8.0 as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 9201/tcp
EXPOSE 9301/tcp

# Set .net 8.0 as build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src
# if main is not found this v is the problem
COPY ["search/", "Search/"]
COPY ["LedditModels/LedditModels.csproj", "LedditModels/"]
RUN dotnet restore "Search/Search.csproj"
COPY . .

WORKDIR /src/Search
RUN dotnet build "Search.csproj" -c Release -o /app

FROM build-env AS publish
WORKDIR /src/Search
RUN dotnet publish "Search.csproj" -c Release -o /app

# AS final is unnecessary & purely semantic
FROM base AS final
WORKDIR /app
COPY --from=publish /app .

WORKDIR /app
ENTRYPOINT ["dotnet", "Search.dll"]
