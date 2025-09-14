Place your MyStore.sql here as MyStore.sql.

docker-compose.yml mounts this folder to /init in the sql tools container and runs:

  sqlcmd -S sqlserver -U sa -P "$SA_PASSWORD" -C -i /init/MyStore.sql

Tips:
- Ensure the script creates the MyStore database and tables, or switches to it:
  IF DB_ID('MyStore') IS NULL CREATE DATABASE MyStore; GO
  USE MyStore; GO
- The script should be idempotent if you expect retries.

