FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY *.sln ./

# Add any new projects to the build here
COPY Chat.Web/Chat.Web/*.csproj ./Chat.Web/Chat.Web/
COPY Chat.Web/Chat.Web.Client/*.csproj ./Chat.Web/Chat.Web.Client/
COPY Chat.Data/*.csproj ./Chat.Data/
COPY Chat.UnitTests/*.csproj ./Chat.UnitTests/
COPY Chat.IntegrationTests/*.csproj ./Chat.IntegrationTests/
COPY Chat.Observability/*.csproj ./Chat.Observability/
COPY Chat.ImageProcessing/*.csproj ./Chat.ImageProcessing/
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet build
