# Public address of the deployed application.
output "app_fqdn" {
  value = azurerm_container_app.this.latest_revision_fqdn
}

output "app_url" {
  value = "https://${azurerm_container_app.this.latest_revision_fqdn}"
}
