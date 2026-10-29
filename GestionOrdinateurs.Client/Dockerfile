# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY ./GestionOrdinateurs.Client/GestionOrdinateurs.Client.csproj ./GestionOrdinateurs.Client/
COPY ./GestionOrdinateurs.Shared/GestionOrdinateurs.Shared.csproj ./GestionOrdinateurs.Shared/
RUN dotnet restore ./GestionOrdinateurs.Client

COPY ./GestionOrdinateurs.Client/ ./GestionOrdinateurs.Client/
COPY ./GestionOrdinateurs.Shared/ ./GestionOrdinateurs.Shared/
RUN dotnet publish ./GestionOrdinateurs.Client -c Release -o out

# Serveur de fichiers statiques (Blazor WebAssembly)
FROM nginx:alpine AS runtime
COPY --from=build /app/out/wwwroot /usr/share/nginx/html
COPY ./GestionOrdinateurs.Client/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 5000