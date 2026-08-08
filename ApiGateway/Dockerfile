# Build context: repo root (docker build -f ApiGateway/Dockerfile .)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ApiGateway/ApiGateway.csproj", "ApiGateway/"]
RUN dotnet restore "ApiGateway/ApiGateway.csproj"

COPY ApiGateway/ ApiGateway/
WORKDIR /src/ApiGateway
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# ASPNETCORE_ENVIRONMENT=Docker để Program.cs nạp thêm ocelot.Docker.json
# (đổi Host của downstream routes từ "localhost" sang tên service trong docker-compose)
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Docker
# Tắt FileSystemWatcher theo dõi appsettings.json - tránh hết quota inotify
# trên container giới hạn tài nguyên (đã gặp crash 139 thật trên Render free tier).
ENV DOTNET_hostBuilder__reloadConfigOnChange=false
EXPOSE 8080

ENTRYPOINT ["dotnet", "ApiGateway.dll"]
