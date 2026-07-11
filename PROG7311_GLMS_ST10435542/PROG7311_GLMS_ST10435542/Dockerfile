# ---------- Stage 1: build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first (cached layer), then copy the rest of the source
COPY PROG7311_GLMS_ST10435542/PROG7311_GLMS_ST10435542.csproj PROG7311_GLMS_ST10435542/
RUN dotnet restore PROG7311_GLMS_ST10435542/PROG7311_GLMS_ST10435542.csproj

COPY PROG7311_GLMS_ST10435542/ PROG7311_GLMS_ST10435542/
RUN dotnet publish PROG7311_GLMS_ST10435542/PROG7311_GLMS_ST10435542.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---------- Stage 2: runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PROG7311_GLMS_ST10435542.dll"]
