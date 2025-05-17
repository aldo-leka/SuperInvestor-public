FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Install Node.js
RUN apt-get update && \
    apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs && \
    npm install -g npm@latest

# Copy everything and restore
COPY . .
RUN dotnet restore

# Build frontend assets
RUN npm install
RUN npm run build

# Build and publish
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .
COPY --from=build /app/wwwroot/dist ./wwwroot/dist

# Configure ASP.NET Core to properly handle HTTPS behind proxy
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
ENV ASPNETCORE_URLS=http://+:3000
ENV ASPNETCORE_HTTPS_PORT=443
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_REVERSEPROXY_FORWARDED_HEADERS_ENABLED=true
ENV ASPNETCORE_REVERSEPROXY_HTTPS=true

EXPOSE 3000
ENTRYPOINT ["dotnet", "SuperInvestor.dll"]