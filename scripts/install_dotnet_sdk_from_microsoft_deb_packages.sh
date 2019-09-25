#!/usr/bin/env bash
set -e

# taken from https://www.microsoft.com/net/download/linux-package-manager/ubuntu18-04/sdk-current
sudo apt install -y wget
wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt install -y apt-transport-https
sudo apt update

sudo apt install -y dotnet-sdk-2.1

dotnet --version
