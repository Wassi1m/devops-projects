FROM python:3.10-slim

WORKDIR /app

# 🔥 Installer dépendances système utiles
RUN apt-get update && apt-get install -y \
    gcc \
    && rm -rf /var/lib/apt/lists/*

# Copier seulement requirements (cache Docker)
COPY requirements.txt .

RUN pip install --no-cache-dir -r requirements.txt

# Copier le reste
COPY . .

CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000"]
