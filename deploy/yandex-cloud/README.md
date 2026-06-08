# Deploy to Yandex Cloud (VM + Docker Compose)

This guide deploys the project to a Yandex Cloud Compute VM using Docker Compose.

You asked for: VM deployment, PostgreSQL in Docker, and your local `yc` is not configured.

Recommended path for the first deploy: **create VM in web console → SSH → git clone → `docker compose up --build`**.

## Prerequisites

- Yandex Cloud account + a `folder` in a `cloud`
- An SSH key pair (public key will be added to the VM)
- Git access to your repository (public repo or a deploy key / token)
- A domain (optional, for HTTPS)

## Option A (Recommended): build on VM from repository

### A1) Create a VM (via YC web console)

- Compute Cloud → Virtual machines → Create VM
- OS: Ubuntu 22.04/24.04
- Disk: from 30 GB (depends on Docker layers)
- CPU/RAM: start from 2 vCPU / 4 GB RAM (more is better for build)
- Add your **SSH public key** to metadata (SSH keys)

Security group / firewall:

- Allow inbound TCP `22` (SSH)
- Allow inbound TCP `5000` (TaskFlow web)
- Allow inbound TCP `8000` (AI FastAPI)
- (Optional) allow `5432` only if you need external DB access (usually keep it closed)

### A2) Install Docker on the VM

SSH to the VM and install Docker Engine + Compose plugin (use official Docker docs for Ubuntu).

### A3) Clone repo and run

On the VM:

1) Clone your repo into a folder, e.g. `/opt/taskflow`

2) Create `.env` next to the compose file (example below)

3) Run Compose (note: in this repo the file name is `Docker-compose.yml`):

- `docker compose --env-file .env -f Docker-compose.yml up -d --build`

Check:

- Web: `http://<vm-public-ip>:5000`
- AI: `http://<vm-public-ip>:8000/health`

## Option B: use Yandex Container Registry (YCR) + prebuilt images

This option avoids building on the VM and is better for repeated deploys.

Prerequisites (local machine):

- `yc` CLI installed and initialized (`yc init`)
- A YCR registry

### B1) Create a Container Registry

Create a registry and configure Docker auth (see YC docs for exact commands for your OS):

- Create Yandex Container Registry (YCR)
- Login Docker to YCR (`yc container registry configure-docker`)

You will push two images:

- `taskflow-web` (OnlineAPI)
- `taskflow-ai` (AIFastApi, used by both API and worker)

### B2) Build and push images

From repo root:

1. Build `taskflow-ai` image from `AIFastApi`
2. Build `taskflow-web` image from `OnlineAPI`
3. Tag them with your YCR registry (e.g. `cr.yandex/<registry-id>/taskflow-ai:latest`)
4. Push both images to YCR

### B3) Prepare `.env` for the VM

Create an `.env` file on the VM (or locally and upload) with at least:

- `POSTGRES_DB=taskflow`
- `POSTGRES_USER=taskflow`
- `POSTGRES_PASSWORD=...`
- `AI_SERVICE_URL=http://taskflow-ai:8000`
- `TASKFLOW_AI_IMAGE=cr.yandex/<registry-id>/taskflow-ai:latest`
- `TASKFLOW_WEB_IMAGE=cr.yandex/<registry-id>/taskflow-web:latest`

If you plan to use Managed PostgreSQL (recommended for production), you will also need:

- `ConnectionStrings__DefaultConnection=Host=<managed-host>;Port=5432;Database=...;Username=...;Password=...;Ssl Mode=Require;Trust Server Certificate=true`

### B4) Create a VM and install Docker

Create a Compute VM (Ubuntu) and install:

- Docker Engine + Docker Compose plugin

Then copy these files to the VM:

- `deploy/yandex-cloud/docker-compose.yc.yml`
- `.env`

### B5) Run on the VM

On the VM, in the folder containing `docker-compose.yc.yml`:

- `docker compose --env-file .env -f docker-compose.yc.yml up -d`

After startup:

- Web: `http://<vm-public-ip>:5000`
- AI: `http://<vm-public-ip>:8000/health`

## Notes

- Current repo `Docker-compose.yml` is aimed at local dev (build from sources).
- `docker-compose.yc.yml` uses prebuilt images from YCR for cloud deployment.
