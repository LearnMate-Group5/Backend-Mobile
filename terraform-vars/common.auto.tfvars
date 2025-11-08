# Common Infrastructure Variables
project_name = "chooy"
aws_region   = "us-east-1"
region       = "us-east-1"

# VPC Configuration
vpc_cidr            = "10.0.0.0/16"
public_subnet_cidrs = ["10.0.1.0/24", "10.0.2.0/24"]
private_subnet_cidr = "10.0.3.0/24"

# EC2 Configuration
instance_type       = "t3.micro"
associate_public_ip = true

# ECS Global Settings
enable_auto_scaling = false

enable_service_connect = true

# HTTPS/SSL Configuration
# To enable HTTPS, replace the certificate_arn value with your ACM certificate ARN
# Example: certificate_arn = "arn:aws:acm:us-east-1:123456789012:certificate/12345678-1234-1234-1234-123456789012"
# To get a certificate, run:
#   aws acm request-certificate --domain-name your-domain.com --validation-method DNS --region us-east-1
certificate_arn       = null  # Set to your ACM certificate ARN to enable HTTPS
enable_https_redirect = true  # When HTTPS is enabled, redirect all HTTP traffic to HTTPS
