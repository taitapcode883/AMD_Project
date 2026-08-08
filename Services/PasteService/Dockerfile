# Build context: repo root (docker build -f Services/PasteService/Dockerfile .)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Services/PasteService/PasteService.csproj", "Services/PasteService/"]
RUN dotnet restore "Services/PasteService/PasteService.csproj"

COPY Services/PasteService/ Services/PasteService/
WORKDIR /src/Services/PasteService
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
# Tắt FileSystemWatcher theo dõi appsettings.json - tránh hết quota inotify
# trên container giới hạn tài nguyên (đã gặp crash 139 thật trên Render free tier).
ENV DOTNET_hostBuilder__reloadConfigOnChange=false
EXPOSE 8080

ENTRYPOINT ["dotnet", "PasteService.dll"]
