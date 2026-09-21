# Stage 1: Build the .NET 8 application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["LexHack.Api.csproj", "./"]
RUN dotnet restore "LexHack.Api.csproj"

# Copy the rest of the code and publish the release build
COPY . .
RUN dotnet publish "LexHack.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Give the restricted 'app' user ownership of the files so SQLite can write to the database
COPY --chown=app:app --from=build /app/publish .

# Render defaults to port 8080
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Explicitly run as the app user
USER app

# When the app starts, run the compiled DLL
ENTRYPOINT ["dotnet", "LexHack.Api.dll"]