#!/bin/sh
rm *.tar.xz
./mkimage-raspbian-buildd.sh
docker build -t occ-sensor-build Dockerfile-buildd
docker build -t occ-sensor -f Dockerfile ../