#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["Suggestions.RestApi/Suggestions.RestApi.csproj", "Suggestions.RestApi/"]
COPY ["Suggestions.Infrastructure/Suggestions.Infrastructure.csproj", "Suggestions.Infrastructure/"]
COPY ["Suggestions.Common/Suggestions.Common.csproj", "Suggestions.Common/"]
COPY ["Suggestions.Logic/Suggestions.Logic.csproj", "Suggestions.Logic/"]
RUN dotnet restore "Suggestions.RestApi/Suggestions.RestApi.csproj"
COPY . .
WORKDIR "/src/Suggestions.RestApi"
RUN dotnet build "Suggestions.RestApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Suggestions.RestApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Install curl
RUN apt-get update && apt-get install curl -y 
ENTRYPOINT ["dotnet", "Suggestions.RestApi.dll"]