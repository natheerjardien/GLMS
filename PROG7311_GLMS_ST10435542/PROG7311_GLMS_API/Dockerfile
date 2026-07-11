# ---------- Stage 1: build ----------
# Multi-stage build: the SDK image compiles the app, but never ships to production.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the project file first so 'dotnet restore' results are cached as their own layer -
# source changes then reuse the restored packages instead of re-downloading them.
COPY PROG7311_GLMS_API/PROG7311_GLMS_API.csproj PROG7311_GLMS_API/
RUN dotnet restore PROG7311_GLMS_API/PROG7311_GLMS_API.csproj

COPY PROG7311_GLMS_API/ PROG7311_GLMS_API/
RUN dotnet publish PROG7311_GLMS_API/PROG7311_GLMS_API.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---------- Stage 2: runtime ----------
# The slim ASP.NET runtime image - no SDK, no source code, much smaller attack surface.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PROG7311_GLMS_API.dll"]
