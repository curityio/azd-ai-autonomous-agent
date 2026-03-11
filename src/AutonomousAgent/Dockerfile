#
# Use Microsoft's multi-stage build recommendations to produce the container
# - https://learn.microsoft.com/en-us/visualstudio/containers/container-build?view=visualstudio#multistage-build
#

#
# The build stage uses the .NET SDK
#
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["AutonomousAgent.csproj", "agent/"]
RUN dotnet restore "agent/AutonomousAgent.csproj"
WORKDIR "/src/agent"
COPY . .
RUN dotnet build "./AutonomousAgent.csproj" -c Release -o /agent/build

#
# The publish stage uses the .NET SDK
#
FROM build AS publish
RUN dotnet publish "./AutonomousAgent.csproj" -c Release -o /agent/publish /p:UseAppHost=false

#
# The deployment stage uses the .NET runtime
#
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /agent
COPY --chown=10001:10000 --from=publish /agent/publish .

RUN groupadd --gid 10000 agentuser \
  && useradd --uid 10001 --gid agentuser --shell /bin/bash --create-home agentuser
USER 10001

CMD ["dotnet", "AutonomousAgent.dll"]
