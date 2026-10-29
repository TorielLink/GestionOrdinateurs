# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY ./GestionOrdinateurs.API/GestionOrdinateurs.API.csproj ./GestionOrdinateurs.API/
COPY ./GestionOrdinateurs.Shared/GestionOrdinateurs.Shared.csproj ./GestionOrdinateurs.Shared/
RUN dotnet restore ./GestionOrdinateurs.API

COPY ./GestionOrdinateurs.API/ ./GestionOrdinateurs.API/
COPY ./GestionOrdinateurs.Shared/ ./GestionOrdinateurs.Shared/
RUN dotnet publish ./GestionOrdinateurs.API -c Release -o out

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:5127
EXPOSE 5127

ENTRYPOINT ["dotnet", "GestionOrdinateurs.API.dll"]