Add Migration

```
$env:ASPNETCORE_ENVIRONMENT="Development"; dotnet ef migrations add Initial -p Backend/Microservices/Subscription.Microservice/src/Infrastructure/Infrastructure.csproj -s Backend/Microservices/Subscription.Microservice/src/Infrastructure/Infrastructure.csproj -c MyDbContext --msbuildprojectextensionspath Backend/Microservices/Subscription.Microservice/src/Infrastructure/Build/obj -- --connection-string="Host=pg-1-database25811.g.aivencloud.com;Port=16026;Database=subscriptiondb;Username=avnadmin;Password=AVNS_iGi4kJJObNRnGdM6BTb;SslMode=Require"
```

```
$env:ASPNETCORE_ENVIRONMENT="Development"; dotnet ef migrations add Initial -p Backend/Microservices/Book.Microservice/src/Infrastructure/Infrastructure.csproj -s Backend/Microservices/Book.Microservice/src/Infrastructure/Infrastructure.csproj -c MyDbContext --msbuildprojectextensionspath Backend/Microservices/Book.Microservice/src/Infrastructure/Build/obj -- --connection-string="Host=pg-1-database25811.g.aivencloud.com;Port=16026;Database=bookdb;Username=avnadmin;Password=AVNS_iGi4kJJObNRnGdM6BTb;SslMode=Require"
```

Apply Migration

```
$env:ASPNETCORE_ENVIRONMENT="Development"; dotnet ef database update -p Backend/Microservices/User.Microservice/src/Infrastructure/Infrastructure.csproj -s Backend/Microservices/User.Microservice/src/Infrastructure/Infrastructure.csproj -c MyDbContext --msbuildprojectextensionspath Backend/Microservices/User.Microservice/src/Infrastructure/Build/obj -- --connection-string="Host=pg-2-database25812.g.aivencloud.com;Port=19217;Database=defaultdb;Username=avnadmin;Password=AVNS_vsIotPLRrxJUhcJlM0m;SslMode=Require"
```

Nuke All Tables

```
DROP SCHEMA public CASCADE;
CREATE SCHEMA public;
GRANT ALL ON SCHEMA public TO postgres;
GRANT ALL ON SCHEMA public TO public;
```

Docker Build

```
docker compose -f 'docker-compose-production.yml' up -d --build
```

# Microservices .NET Template

### 1. Create a GitHub Environment

- Open your repository → Settings → Environments → New environment.
- Name it (for example): `infrastructure-yourname`.

### 2. Configure Environment Variables (vars)

- **AWS_REGION**: `us-east-1`
- **PROJECT_NAME**: `your-proj-name`

### 3. Configure Environment Secrets (secrets)

- **AWS credentials**:
  - **AWS_ACCESS_KEY_ID**
  - **AWS_SECRET_ACCESS_KEY**
- **Terraform variables (paste tfvars contents)**:
  - **TERRAFORM_VARS_COMMON**
  - **TERRAFORM_VARS_RABBITMQ**
  - **TERRAFORM_VARS_REDIS**
  - **TERRAFORM_VARS_USER**

### 4. Prepare tfvars contents for the secrets

- Open files under `terraform-vars/` and copy their contents into the matching secrets:
  - **TERRAFORM_VARS_COMMON** ← `terraform-vars/common.auto.tfvars`
  - **TERRAFORM_VARS_RABBITMQ** ← `terraform-vars/rabbitmq-service.auto.tfvars`
  - **TERRAFORM_VARS_REDIS** ← `terraform-vars/redis-service.auto.tfvars`
  - **TERRAFORM_VARS_USER** ← `terraform-vars/user-service.auto.tfvars`

Notes before you copy:

- Edit the `terraform-vars` files to provide your real values (DB host/port/name/user/password, Redis/RabbitMQ passwords, ECR repository URL with your AWS account ID, etc.).
- Do not commit real secrets to git – paste them into GitHub Environment secrets only.
- Replace placeholders like `your-db-password`, `your-redis-password`, `your-rabbitmq-password` with your own secure values.

### 5. Run “Full Infrastructure Deploy”

- Go to Actions → `Full Infrastructure Deploy` → Run workflow.
- Select your Environment and choose terraform action:
  - **plan**: preview changes
  - **apply**: deploy infrastructure
  - **destroy**: tear down infrastructure
- This workflow will:
  - Bootstrap Terraform backend (S3 + DynamoDB) in AWS.
  - Discover microservices and build/push images to ECR.
  - Generate and merge tfvars from repository and environment secrets.
  - Run Terraform `init` → `validate` → `plan` → `apply`.

### Optional: “Deploy Infrastructure with Terraform”

- Use the `Deploy Infrastructure with Terraform` workflow for plan/apply/destroy without the build step.
- It reuses the same environment vars/secrets and tfvars merge logic as the full deploy.
