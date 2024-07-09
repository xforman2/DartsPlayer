
## Steps

### Step 1: Run PostgreSQL Container

`docker run -d --name darts -p 5556:5432 -e POSTGRES_USER=user -e POSTGRES_PASSWORD=password -e POSTGRES_DB=database postgres`

### Step 2: Apply migrations

`dotnet ef database update`

### Step 3: Run the api
    