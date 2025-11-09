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

# HTTPS Configuration Options
# 
# OPTION 1: CloudFront HTTPS (Recommended - FREE & NO DOMAIN REQUIRED)
# -----------------------------------------------------------------------
# Use CloudFront to get free HTTPS with a *.cloudfront.net domain
# This is the easiest way to enable HTTPS without purchasing a domain or certificate
use_cloudfront_https      = true   # Enable CloudFront for free HTTPS
cloudfront_enable_caching = false  # Disable caching (recommended for APIs)

# CloudFront Access Logging (for debugging - e.g., MoMo IPN not reaching backend)
# Set enable_logging = true and provide your S3 bucket name from bootstrap
cloudfront_enable_logging         = true                    # Enable logging to debug MoMo IPN issues
cloudfront_logging_bucket         = "learnmate-us-east-1-terraform-state"   # Replace with your bootstrap S3 bucket name
cloudfront_logging_prefix         = "cloudfront-logs/"      # Prefix for log files
cloudfront_logging_include_cookies = false                   # Set to true to include cookies in logs

# OPTION 2: ALB with ACM Certificate (Requires Custom Domain)
# -----------------------------------------------------------------------
# To enable HTTPS directly on ALB, you need:
# 1. A custom domain (e.g., your-domain.com)
# 2. An ACM certificate for that domain
# 
# Steps:
#   1. Request certificate: aws acm request-certificate --domain-name your-domain.com --validation-method DNS --region us-east-1
#   2. Add DNS validation records to your domain
#   3. Update certificate_arn below with your ACM certificate ARN
#   4. Set use_cloudfront_https = false
#
# certificate_arn       = "arn:aws:acm:us-east-1:123456789012:certificate/12345678-1234-1234-1234-123456789012"
certificate_arn       = null  # Set to your ACM certificate ARN to enable HTTPS on ALB
enable_https_redirect = true  # When certificate_arn is set, redirect HTTP to HTTPS

# Note: Only one HTTPS option should be active:
# - use_cloudfront_https = true + certificate_arn = null (CloudFront HTTPS - FREE)
# - use_cloudfront_https = false + certificate_arn = "arn:..." (ALB HTTPS - requires domain)
