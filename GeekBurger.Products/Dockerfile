#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["GeekBurger.Products/GeekBurger.Products.csproj", "GeekBurger.Products/"]
COPY ["GeekBurger.Products.Contract/GeekBurger.Products.Contract.csproj", "GeekBurger.Products.Contract/"]
RUN dotnet restore "GeekBurger.Products/GeekBurger.Products.csproj"
COPY . .
WORKDIR "/src/GeekBurger.Products"
RUN dotnet build "GeekBurger.Products.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GeekBurger.Products.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GeekBurger.Products.dll"]