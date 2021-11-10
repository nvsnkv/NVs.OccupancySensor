#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM occ-sensor-buildd AS deps
RUN apt-get update;
RUN apt-get install -y wget
WORKDIR /usr/share/dotnet
WORKDIR /usr/share
RUN wget https://download.visualstudio.microsoft.com/download/pr/1f85b038-9917-4d0a-8485-5dc86510eec7/a7555924fe292c6c2140893f066abe65/dotnet-sdk-6.0.100-linux-arm.tar.gz -O /usr/share/sdk.tar.gz
RUN tar zxf /usr/share/sdk.tar.gz -C /usr/share/dotnet
ENV DOTNET_ROOT=/usr/share/dotnet
ENV PATH="$PATH:/usr/share/dotnet"
WORKDIR /vendor
RUN apt-get install -y git
RUN git clone --depth 1 --branch 4.5.4 https://github.com/emgucv/emgucv
WORKDIR /vendor/emgucv
RUN git submodule update --init --recursive
WORKDIR /vendor/emgucv/platforms/raspberry_pi_os
RUN DEBIAN_FRONTEND="noninteractive" apt-get -y install tzdata
RUN apt-get install -y sudo
RUN yes | ./apt_install_dependency
RUN ./cmake_configure

FROM deps as emgucv
WORKDIR /libs
COPY --from=deps /vendor/emgucv/libs .
RUN mv arm/* .
ENV LD_LIBRARY_PATH="$LD_LIBRARY_PATH:/libs"
ENV SUDO_FORCE_REMOVE=yes
RUN apt remove -y sudo git wget
RUN apt-get clean autoclean
RUN apt-get autoremove --yes
RUN rm -rf /var/lib/{apt,dpkg,cache,log}/
RUN rm -rf /vendor/emgucv
RUN rm /usr/share/sdk.tar.gz

FROM deps AS build
WORKDIR /src
COPY ["NVs.OccupancySensor.API/NVs.OccupancySensor.API.csproj", "NVs.OccupancySensor.API/"]
COPY ["NVs.OccupancySensor.CV/NVs.OccupancySensor.CV.csproj", "NVs.OccupancySensor.CV/"]
RUN dotnet restore "NVs.OccupancySensor.CV/NVs.OccupancySensor.CV.csproj"
RUN dotnet restore "NVs.OccupancySensor.API/NVs.OccupancySensor.API.csproj"
COPY . .
WORKDIR "/src/NVs.OccupancySensor.API"
RUN dotnet build "NVs.OccupancySensor.API.csproj" -c Release -o /app/build -r linux-arm 

FROM build AS test
RUN dotnet test --no-build

FROM build AS publish
RUN dotnet publish "NVs.OccupancySensor.API.csproj" -c Release -o /app/publish -r linux-arm

FROM emgucv AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 80
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NVs.OccupancySensor.API.dll"]