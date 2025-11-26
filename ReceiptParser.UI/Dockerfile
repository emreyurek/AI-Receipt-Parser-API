# -- BÝRÝNCÝ AÞAMA: PROJEYÝ DERLEME (BUILD) --
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Proje dosyasýný ve referanslarý kopyala
COPY ["ReceiptParser.UI.csproj", "ReceiptParser.UI/"]
RUN dotnet restore "ReceiptParser.UI/ReceiptParser.UI.csproj"

# Tüm Frontend kaynak kodunu kopyala
COPY . "ReceiptParser.UI/"
WORKDIR "/src/ReceiptParser.UI"

# Blazor WebAssembly uygulamasýný yayýnla
RUN dotnet publish "ReceiptParser.UI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# -- ÝKÝNCÝ AÞAMA: ÇALIÞTIRMA (RUNTIME) --
# Basit bir web sunucusu (nginx) imajýný temel alarak statik dosyalarý yayýnlayacaðýz.
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Build aþamasýndan yayýnlanmýþ statik dosyalarý (wwwroot) kopyala
COPY --from=build /app/publish/wwwroot .

# Web sunucusu 80 portunda dinler.