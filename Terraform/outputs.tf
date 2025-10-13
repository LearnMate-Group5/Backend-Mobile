output "alb_dns_name" {
  description = "Public DNS name for the Application Load Balancer."
  value       = module.alb.alb_dns_name
}

