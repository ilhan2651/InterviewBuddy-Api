# 1. Build Aşaması
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Proje dosyalarını kopyala ve bağımlılıkları yükle (Cache optimizasyonu için)
COPY ["Buddy.Api/Buddy.Api.csproj", "Buddy.Api/"]
COPY ["Buddy.Application/Buddy.Application.csproj", "Buddy.Application/"]
COPY ["Buddy.Domain/Buddy.Domain.csproj", "Buddy.Domain/"]
COPY ["Buddy.Infrastructure/Buddy.Infrastructure.csproj", "Buddy.Infrastructure/"]
COPY ["Buddy.Persistence/Buddy.Persistence.csproj", "Buddy.Persistence/"]
RUN dotnet restore "Buddy.Api/Buddy.Api.csproj"

# Tüm kodları kopyala ve build al
COPY . .
WORKDIR "/src/Buddy.Api"
RUN dotnet build "Buddy.Api.csproj" -c Release -o /app/build

# Uygulamayı yayınla (publish)
FROM build AS publish
RUN dotnet publish "Buddy.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Çalışma Aşaması (Sadece çalışma zamanı kütüphaneleri - Daha hafif imaj)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Python 3 + edge-tts kurulumu (Ücretsiz TTS fallback için zorunlu)
RUN apt-get update && \
    apt-get install -y python3 python3-pip --no-install-recommends && \
    pip3 install edge-tts --break-system-packages && \
    apt-get clean && rm -rf /var/lib/apt/lists/*

# Audio klasörünü oluştur
RUN mkdir -p /app/wwwroot/audio/ai

# Uygulamanın dinleyeceği port (Docker içinde)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Buddy.Api.dll"]

