#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
# container for development!

FROM ubuntu:20.04 AS emgucv-src
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
RUN git checkout 4.5.1
RUN git submodule update --init --recursive
WORKDIR /vendor/emgucv/platforms/ubuntu/20.04
RUN apt-get install -y sudo
RUN yes | ./apt_install_dependency.sh
RUN ./cmake_configure.sh

FROM emgucv-src as emgucv
WORKDIR /libs
COPY --from=emgucv-src /vendor/emgucv/libs .
ENV LD_LIBRARY_PATH=/libs
ENV SUDO_FORCE_REMOVE=yes
RUN apt remove -y sudo git wget
RUN apt-get clean autoclean
RUN apt-get autoremove --yes
RUN rm -rf /var/lib/{apt,dpkg,cache,log}/

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
RUN dotnet build "NVs.OccupancySensor.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NVs.OccupancySensor.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NVs.OccupancySensor.API.dll"]