FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Sửa lại đường dẫn copy cho đúng cấu trúc folder của bạn
COPY ["ConnectDB/ConnectDB.csproj", "ConnectDB/"]
RUN dotnet restore "ConnectDB/ConnectDB.csproj"

COPY . .
WORKDIR "/src/ConnectDB"
RUN dotnet build "ConnectDB.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConnectDB.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "ConnectDB.dll"]