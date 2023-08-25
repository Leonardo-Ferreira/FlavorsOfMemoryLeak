#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["FlavorsOfMemoryLeak.Api/FlavorsOfMemoryLeak.Api.csproj", "FlavorsOfMemoryLeak.Api/"]
RUN dotnet restore "FlavorsOfMemoryLeak.Api/FlavorsOfMemoryLeak.Api.csproj"
COPY . .
WORKDIR "/src/FlavorsOfMemoryLeak.Api"
RUN dotnet build "FlavorsOfMemoryLeak.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FlavorsOfMemoryLeak.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
RUN apk add gcompat
RUN wget https://aka.ms/dotnet-counters/linux-x64 -O dotnet-counters
RUN chmod a+x dotnet-counters
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FlavorsOfMemoryLeak.Api.dll"]
