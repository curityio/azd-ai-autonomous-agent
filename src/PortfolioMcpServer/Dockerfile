#
# Use Microsoft's multi-stage build recommendations to produce the container
# - https://learn.microsoft.com/en-us/visualstudio/containers/container-build?view=visualstudio#multistage-build
#

#
# The build stage uses the .NET SDK
#
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["PortfolioMcpServer.csproj", "mcp/"]
RUN dotnet restore "mcp/PortfolioMcpServer.csproj"
WORKDIR "/src/mcp"
COPY . .
RUN dotnet build "./PortfolioMcpServer.csproj" -c Release -o /mcp/build

#
# The publish stage uses the .NET SDK
#
FROM build AS publish
RUN dotnet publish "./PortfolioMcpServer.csproj" -c Release -o /mcp/publish /p:UseAppHost=false

#
# The deployment stage uses the .NET runtime
#
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /mcp
COPY --chown=10001:10000 --from=publish /mcp/publish .

RUN groupadd --gid 10000 mcpuser \
  && useradd --uid 10001 --gid mcpuser --shell /bin/bash --create-home mcpuser
USER 10001

CMD ["dotnet", "PortfolioMcpServer.dll"]
