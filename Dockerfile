FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution & project files
COPY BlazorApp1.slnx ./
COPY BlazorApp1/BlazorApp1.csproj ./BlazorApp1/
RUN dotnet restore

# Copy all source files & build
COPY . .
RUN dotnet publish ./BlazorApp1/BlazorApp1.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BlazorApp1.dll"]
