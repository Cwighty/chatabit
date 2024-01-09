FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["chat.tests/chat.tests.csproj", "chat.tests/"]
RUN dotnet restore "chat.tests/chat.tests.csproj"
COPY . .
RUN dotnet test