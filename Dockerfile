FROM mcr.microsoft.com/dotnet/sdk:6.0 as build

COPY Diffcord /app

RUN dotnet build /app/Diffcord.csproj /p:Configuration=Release -o /artifact

FROM mcr.microsoft.com/dotnet/runtime:6.0

COPY --from=build /artifact /app

ENTRYPOINT ["dotnet", "/app/Diffcord.dll"]