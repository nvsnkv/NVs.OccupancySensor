#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM ubuntu:20.04 AS emgucv
WORKDIR /key
RUN apt-get update;
RUN apt-get install -y wget
RUN wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN apt-get install -y apt-transport-https && apt-get update && apt-get install -y aspnetcore-runtime-3.1
WORKDIR /vendor
RUN apt-get install -y git
RUN git clone https://github.com/emgucv/emgucv emgucv
WORKDIR /vendor/emgucv
RUN git submodule update --init --recursive
WORKDIR /vendor/emgucv/platforms/ubuntu/20.04
RUN apt-get install -y sudo
RUN yes | ./apt_install_dependency.sh
RUN ./cmake_configure.sh

FROM emgucv AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["NVs.OccupancySensor.API/NVs.OccupancySensor.API.csproj", "NVs.OccupancySensor.API/"]
RUN dotnet restore "NVs.OccupancySensor.API/NVs.OccupancySensor.API.csproj"
COPY . .
WORKDIR "/src/NVs.OccupancySensor.API"
RUN dotnet build "NVs.OccupancySensor.API.csproj" -c Release -o /app/build --runtime linux-x64

FROM build AS publish
RUN dotnet publish "NVs.OccupancySensor.API.csproj" -c Release -o /app/publish --runtime linux-x64

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ["NVs.OccupancySensor.API/lib/libcvextern.so", "libcvextern.so"]
ENTRYPOINT ["dotnet", "NVs.OccupancySensor.API.dll"]