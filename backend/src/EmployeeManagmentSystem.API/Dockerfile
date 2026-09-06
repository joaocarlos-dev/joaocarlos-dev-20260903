FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src

COPY EmployeeManagmentSystem.API/EmployeeManagmentSystem.API.csproj EmployeeManagmentSystem.API/
COPY EmployeeManagmentSystem.Application/EmployeeManagmentSystem.Application.csproj EmployeeManagmentSystem.Application/
COPY EmployeeManagmentSystem.Domain/EmployeeManagmentSystem.Domain.csproj EmployeeManagmentSystem.Domain/
COPY EmployeeManagmentSystem.Infrastructure/EmployeeManagmentSystem.Infrastructure.csproj EmployeeManagmentSystem.Infrastructure/
COPY EmployeeManagmentSystem.Tests/EmployeeManagmentSystem.Tests.csproj EmployeeManagmentSystem.Tests/
RUN dotnet restore EmployeeManagmentSystem.Tests/EmployeeManagmentSystem.Tests.csproj
RUN dotnet restore EmployeeManagmentSystem.API/EmployeeManagmentSystem.API.csproj

COPY . .

FROM restore AS test
ENTRYPOINT ["dotnet", "test", "EmployeeManagmentSystem.Tests/EmployeeManagmentSystem.Tests.csproj", "--configuration", "Release", "--no-restore", "--logger", "console;verbosity=normal"]

FROM restore AS publish
RUN dotnet publish EmployeeManagmentSystem.API/EmployeeManagmentSystem.API.csproj --configuration Release --no-restore --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=publish /app/publish .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "EmployeeManagmentSystem.API.dll"]
