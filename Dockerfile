# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем все .csproj файлы для восстановления зависимостей
COPY lumires.Domain/lumires.Domain.csproj                               lumires.Domain/
COPY lumires.Core/lumires.Core.csproj                                   lumires.Core/
COPY lumires.Infrastructure/lumires.Infrastructure.csproj               lumires.Infrastructure/
COPY lumires.Api/lumires.Api.csproj                                     lumires.Api/
COPY lumires.Composition/lumires.Composition.csproj                     lumires.Composition/
COPY lumires.ServiceDefaults/lumires.ServiceDefaults.csproj             lumires.ServiceDefaults/

# Восстанавливаем зависимости
RUN dotnet restore lumires.Composition/lumires.Composition.csproj

# Копируем весь исходный код
COPY lumires.Domain/            lumires.Domain/
COPY lumires.Core/              lumires.Core/
COPY lumires.Infrastructure/    lumires.Infrastructure/
COPY lumires.Api/               lumires.Api/
COPY lumires.Composition/       lumires.Composition/
COPY lumires.ServiceDefaults/   lumires.ServiceDefaults/

# Собираем и публикуем
RUN dotnet publish lumires.Composition/lumires.Composition.csproj \
    -c Release \
    -o /app/publish

# Этап запуска
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "lumires.Composition.dll"]