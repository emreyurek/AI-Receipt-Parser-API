# -- BÝRÝNCÝ AÞAMA: PROJEYÝ DERLEME (BUILD) --
# Hafif bir .NET SDK imajýný temel alýyoruz.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Proje dosyasýný (csproj) kopyala ve restore et. 
# Bu, baðýmlýlýklarý çabucak önbelleðe alýr.
COPY ["ReceiptParserAPI.csproj", "ReceiptParserAPI/"]
RUN dotnet restore "ReceiptParserAPI/ReceiptParserAPI.csproj"

# Tüm Backend kaynak kodunu kopyala
COPY . "ReceiptParserAPI/"
WORKDIR "/src/ReceiptParserAPI"

# Uygulamayý Release modunda yayýnla (Publish)
RUN dotnet publish "ReceiptParserAPI.csproj" -c Release -o /app/publish

# -- ÝKÝNCÝ AÞAMA: ÇALIÞTIRMA (RUNTIME) --
# Daha küçük ve güvenli bir Runtime imajýna geçiyoruz (sadece çalýþtýracak dosyalar var).
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Derlenmiþ dosyalarý kopyala
COPY --from=build /app/publish .

# Uygulamayý çalýþtýrma komutu (Kestrel sunucusunu baþlatýr)
ENTRYPOINT ["dotnet", "ReceiptParserAPI.dll"]