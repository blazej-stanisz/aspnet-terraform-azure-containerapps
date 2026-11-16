# Public address of the deployed application.
output "app_fqdn" {
  value = azurerm_container_app.this.latest_revision_fqdn
}

output "app_url" {
  value = "https://${azurerm_container_app.this.latest_revision_fqdn}"
}

# Stable public address of the application, independent of the revision.
output "app_stable_url" {
  value = "https://${azurerm_container_app.this.ingress[0].fqdn}"
}
