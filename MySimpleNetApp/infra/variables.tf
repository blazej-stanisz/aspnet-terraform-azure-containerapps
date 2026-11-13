variable "name_prefix" {
  description = "Prefix used in the name of every created resource."
  type        = string
  default     = "mysimple"
}
variable "environment" {
  description = "Deployment environment. Allowed values: dev, release."
  type        = string
  default     = "dev"
  validation {
    condition     = contains(["dev", "release"], var.environment)
    error_message = "Allowed values are: dev or release."
  }
}
variable "resource_group_name" {
  description = "Resource group for this deployment, created outside Terraform."
  type        = string
  default     = "rg-mysimple-dev"
}
variable "container_app_environment_name" {
  description = "Existing Container Apps environment shared by all projects in the subscription."
  type        = string
  default     = "managedEnvironment-rgmynettestapp2-93c7"
}
variable "container_app_environment_resource_group" {
  description = "Resource group that holds the shared Container Apps environment."
  type        = string
  default     = "rg-global"
}
variable "dockerhub_username" {
  description = "Docker Hub account that owns the application image."
  type        = string
  default     = "blazejs"
}
variable "image_name" {
  description = "Docker Hub repository name."
  type        = string
  default     = "mysimplenetapp_ui"
}
variable "image_tag" {
  description = "Image tag deployed to this environment."
  type        = string
  default     = "1.4"
}
variable "database_name" {
  description = "PostgreSQL database name used by the application."
  type        = string
  default     = "todos"
}
variable "database_user" {
  description = "PostgreSQL user name used by the application."
  type        = string
  default     = "app"
}