FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY AddressBook.sln ./
COPY src/AddressBook.Api/AddressBook.Api.csproj src/AddressBook.Api/
COPY tests/AddressBook.Api.Tests/AddressBook.Api.Tests.csproj tests/AddressBook.Api.Tests/
RUN dotnet restore

COPY . .
RUN dotnet publish src/AddressBook.Api/AddressBook.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AddressBook.Api.dll"]
