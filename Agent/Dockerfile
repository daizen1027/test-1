#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
#COPY ["WebApplication1/WebApplication1.csproj", "WebApplication1/"]
COPY ["Agent.csproj", "Agent/"]
RUN dotnet restore "Agent/Agent.csproj"
WORKDIR "/src/Agent"
COPY . .
#upside down up=>COPY . . down=>WORKDIR "/src/WebApplication1"
RUN dotnet build "Agent.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Agent.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "WebApplication1.dll"]
CMD ASPNETCORE_URLS=http://*:$PORT dotnet Agent.dll