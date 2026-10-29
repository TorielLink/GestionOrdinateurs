FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY ./AuthentificationApi/AuthentificationApi.csproj ./AuthentificationApi/
COPY ./GestionOrdinateurs.Shared/GestionOrdinateurs.Shared.csproj ./GestionOrdinateurs.Shared/
RUN dotnet restore ./AuthentificationApi

COPY ./AuthentificationApi/ ./AuthentificationApi/
COPY ./GestionOrdinateurs.Shared/ ./GestionOrdinateurs.Shared/
RUN dotnet publish ./AuthentificationApi -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 5157
ENTRYPOINT ["dotnet", "AuthentificationApi.dll"]