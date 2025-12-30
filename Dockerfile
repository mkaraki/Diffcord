FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

COPY Diffcord /app

RUN dotnet build /app/Diffcord.csproj /p:Configuration=Release -o /artifact

FROM dhi.io/dotnet:10.0-alpine3.22

COPY --from=build /artifact /app

WORKDIR /app

VOLUME /app/config.yaml

ENTRYPOINT ["dotnet", "/app/Diffcord.dll"]