# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY . .
RUN dotnet restore GPA.Api/GPA.Api.csproj

RUN dotnet publish -c Release -o /app/publish --no-restore GPA.Api/GPA.Api.csproj

# copy everything else and build app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install dependencies
RUN apt-get update && \
    apt-get install -y libgdiplus && \
    ln -s /usr/lib/libgdiplus.so /usr/lib/gdiplus.dll
    
# Copy the published application files
COPY --from=build /app/publish .

# Set the LD_LIBRARY_PATH environment variable
ENV LD_LIBRARY_PATH=/app/libs/dinktopdflibs/64\ bit:$LD_LIBRARY_PATH

# Expose the port the app runs on
EXPOSE 8080

# Run the application
ENTRYPOINT ["dotnet", "GPA.Api.dll"]