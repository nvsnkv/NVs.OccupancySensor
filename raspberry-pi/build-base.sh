#!/bin/sh
./mkimage-raspbian.sh
docker build -t occ-sensor-paspbian-base -f Dockerfile-base .