FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Biblioteca.Dominio/Tienda.Dominio.csproj Biblioteca.Dominio/
COPY Biblioteca.Utilitarios/Tienda.Utilitarios.csproj Biblioteca.Utilitarios/
COPY Biblioteca.AccesoDatos/Tienda.AccesoDatos.csproj Biblioteca.AccesoDatos/
COPY Biblioteca.LogicaNegocio/Tienda.LogicaNegocio.csproj Biblioteca.LogicaNegocio/
COPY BibliotecaLDRFApis/TiendaelBatarazoApis.csproj BibliotecaLDRFApis/

RUN dotnet restore BibliotecaLDRFApis/TiendaelBatarazoApis.csproj

COPY . .
RUN dotnet publish BibliotecaLDRFApis/TiendaelBatarazoApis.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "TiendaelBatarazoApis.dll"]
