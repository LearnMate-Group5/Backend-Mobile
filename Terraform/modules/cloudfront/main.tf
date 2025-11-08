locals {
  # Origin ID for the ALB
  alb_origin_id = "${var.project_name}-alb-origin"
}

# Look up AWS managed CachingDisabled policy
data "aws_cloudfront_cache_policy" "caching_disabled" {
  name = "Managed-CachingDisabled"
}

resource "aws_cloudfront_distribution" "alb_distribution" {
  enabled             = true
  is_ipv6_enabled     = true
  comment             = "CloudFront distribution for ${var.project_name} ALB with HTTPS"
  price_class         = var.price_class
  http_version        = "http2and3"
  wait_for_deployment = true

  depends_on = [data.aws_cloudfront_cache_policy.caching_disabled]

  # Origin configuration - pointing to ALB
  origin {
    domain_name = var.alb_dns_name
    origin_id   = local.alb_origin_id

    custom_origin_config {
      http_port              = 80
      https_port             = 443
      origin_protocol_policy = "http-only" # ALB doesn't have HTTPS yet, so use HTTP
      origin_ssl_protocols   = ["TLSv1.2"]
      origin_read_timeout    = 60
      origin_keepalive_timeout = 5
    }

    # Custom headers to identify CloudFront requests at ALB (optional)
    custom_header {
      name  = "X-Custom-Origin"
      value = var.project_name
    }
  }

  # Default cache behavior
  default_cache_behavior {
    allowed_methods        = var.allowed_methods
    cached_methods         = var.cached_methods
    target_origin_id       = local.alb_origin_id
    compress               = var.compress
    viewer_protocol_policy = var.viewer_protocol_policy

    # Attach BOTH modern policies
    cache_policy_id          = data.aws_cloudfront_cache_policy.caching_disabled.id
    origin_request_policy_id = aws_cloudfront_origin_request_policy.include_cloudfront_headers.id
  }

  # Restrictions (no geographic restrictions by default)
  restrictions {
    geo_restriction {
      restriction_type = "none"
    }
  }

  # SSL/TLS certificate configuration
  # CloudFront provides a default *.cloudfront.net certificate for free
  viewer_certificate {
    cloudfront_default_certificate = true
    minimum_protocol_version       = "TLSv1.2_2021"
  }

  tags = {
    Name        = "${var.project_name}-cloudfront-distribution"
    Project     = var.project_name
    Environment = "production"
    Purpose     = "HTTPS termination for ALB"
  }
}

# Create Origin Request Policy to include CloudFront headers
resource "aws_cloudfront_origin_request_policy" "include_cloudfront_headers" {
  name    = "${var.project_name}-cloudfront-headers-policy"
  comment = "Policy to forward CloudFront-Forwarded-Proto and Host header to origin"

  cookies_config {
    cookie_behavior = "all"
  }

  headers_config {
    header_behavior = "allViewerAndWhitelistCloudFront"
    headers {
      items = [
        "CloudFront-Forwarded-Proto",
        "CloudFront-Viewer-Country",
        "CloudFront-Is-Mobile-Viewer",
        "CloudFront-Is-Tablet-Viewer",
        "CloudFront-Is-Desktop-Viewer"
      ]
    }
  }

  query_strings_config {
    query_string_behavior = "all"
  }
}
