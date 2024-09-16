# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY GPA.Api/GPA.Api.csproj GPA.Api/
COPY GPA.Dtos/GPA.Dtos.csproj GPA.Dtos/
COPY GPA.Entities/GPA.Entities.csproj GPA.Entities/
COPY GPA.Utils/GPA.Utils.csproj GPA.Utils/
COPY GPA.Data/GPA.Data.csproj GPA.Data/
COPY GPA.Services/GPA.Services.csproj GPA.Services/
COPY GPA.Tests/GPA.Tests.csproj GPA.Tests/
COPY GPA.sln .
RUN dotnet restore GPA.Api/GPA.Api.csproj

# copy everything else and build app
COPY . .

RUN dotnet publish GPA.Api/GPA.Api.csproj -c Release -o /app/publish --no-restore

# build the app from the build image
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