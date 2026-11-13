# Values reused by more than one resource.
locals {
  tags = {
    project     = "aspnet-terraform-azure-containerapps"
    environment = var.environment
    managed_by  = "terraform"
  }
}
# Resource group for this deployment. Created outside Terraform during bootstrap.
data "azurerm_resource_group" "this" {
  name = var.resource_group_name
}
# The subscription allows only one Container Apps environment, so every deployment
# reuses the existing one instead of creating a new one.
data "azurerm_container_app_environment" "this" {
  name                = var.container_app_environment_name
  resource_group_name = var.container_app_environment_resource_group
}
# Password generated once and stored in the remote state.
# Special characters are disabled because the value is embedded in a connection string.
resource "random_password" "postgres" {
  length  = 24
  special = false
}
resource "azurerm_container_app" "this" {
  name                         = "ca-${var.name_prefix}-${var.environment}"
  container_app_environment_id = data.azurerm_container_app_environment.this.id
  resource_group_name          = data.azurerm_resource_group.this.name
  revision_mode                = "Single"
  # Secret values are hidden from the environment variable list in the portal.
  secret {
    name  = "postgres-password"
    value = random_password.postgres.result
  }
  secret {
    name  = "db-connection-string"
    value = "Host=localhost;Port=5432;Database=${var.database_name};Username=${var.database_user};Password=${random_password.postgres.result}"
  }
  template {
    # Scale to zero when there is no traffic, so an idle demo costs almost nothing.
    min_replicas = 0
    max_replicas = 1
    # Application container. Both containers share the same network namespace,
    # so the application reaches the database through localhost.
    container {
      name   = "web"
      image  = "${var.dockerhub_username}/${var.image_name}:${var.image_tag}"
      cpu    = 0.5
      memory = "1Gi"
      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }
      env {
        name        = "ConnectionStrings__Default"
        secret_name = "db-connection-string"
      }
      # The application retries the database connection for about 30 seconds
      # during startup, so the first liveness check is delayed.
      liveness_probe {
        transport               = "HTTP"
        port                    = 8080
        path                    = "/health"
        interval_seconds        = 10
        timeout                 = 5
        failure_count_threshold = 3
        initial_delay           = 20
      }
      readiness_probe {
        transport               = "HTTP"
        port                    = 8080
        path                    = "/health/ready"
        interval_seconds        = 10
        timeout                 = 5
        failure_count_threshold = 3
        success_count_threshold = 1
        initial_delay           = 10
      }
    }
    # PostgreSQL as a sidecar container. Data lives as long as the replica,
    # so it is lost after a restart or after scaling to zero.
    # Acceptable for the dev demo, documented as a limitation.
    container {
      name   = "db"
      image  = "postgres:16-alpine"
      cpu    = 0.5
      memory = "1Gi"
      env {
        name  = "POSTGRES_DB"
        value = var.database_name
      }
      env {
        name  = "POSTGRES_USER"
        value = var.database_user
      }
      env {
        name        = "POSTGRES_PASSWORD"
        secret_name = "postgres-password"
      }
    }
  }
  ingress {
    external_enabled = true
    target_port      = 8080
    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }
  tags = local.tags
}