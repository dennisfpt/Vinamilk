# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file csproj và restore thư viện
COPY ["Vinamilk/Vinamilk.csproj", "Vinamilk/"]
RUN dotnet restore "Vinamilk/Vinamilk.csproj"

# Copy toàn bộ source code và publish
COPY . .
WORKDIR "/src/Vinamilk"
RUN dotnet publish "Vinamilk.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render tự động cấp biến môi trường PORT (thường là 10000 hoặc 8080)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Vinamilk.dll"]