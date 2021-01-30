#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM balenalib/raspberry-pi-debian AS emgucv-src
WORKDIR /install
RUN apt-get update;
RUN apt upgrade -y;
RUN apt-get install -y wget;
RUN wget -q https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
RUN chmod +x dotnet-install.sh
RUN ./dotnet-install.sh --architecture arm --channel 3.1
WORKDIR /vendor
RUN apt-get install -y git
RUN git clone https://github.com/emgucv/emgucv emgucv
WORKDIR /vendor/emgucv
RUN git checkout  4.5.1
RUN git submodule update --init --recursive
WORKDIR /vendor/emgucv/platforms/raspbian
RUN yes | ./apt_install_dependency
RUN ./cmake_configure

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
RUN dotnet restore "NVs.OccupancySensor.API/NVs.OccupancySensor.API.csproj" -r linux-arm
COPY . .
WORKDIR "/src/NVs.OccupancySensor.API"
RUN dotnet build "NVs.OccupancySensor.API.csproj" -c Release -o /app/build -r linux-arm

FROM build AS publish
RUN dotnet publish "NVs.OccupancySensor.API.csproj" -c Release -o /app/publish -r linux-arm

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NVs.OccupancySensor.API.dll"]