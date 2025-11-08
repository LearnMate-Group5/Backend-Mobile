# Origin Request Policy (no Host in CF header list)
resource "aws_cloudfront_origin_request_policy" "include_cloudfront_headers" {
  name    = "${var.project_name}-cloudfront-headers-policy"
  comment = "Forward all viewer headers + selected CloudFront headers"

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

resource "aws_cloudfront_distribution" "alb_distribution" {
  enabled             = true
  is_ipv6_enabled     = true
  comment             = "CloudFront distribution for ${var.project_name} ALB with HTTPS"
  price_class         = var.price_class
  http_version        = "http2and3"
  wait_for_deployment = true

  origin {
    domain_name = var.alb_dns_name
    origin_id   = "${var.project_name}-alb-origin"

    custom_origin_config {
      http_port                = 80
      https_port               = 443
      origin_protocol_policy   = "http-only"
      origin_ssl_protocols     = ["TLSv1.2"]
      origin_read_timeout      = 60
      origin_keepalive_timeout = 5
    }

    custom_header {
      name  = "X-Custom-Origin"
      value = var.project_name
    }
  }

  default_cache_behavior {
    target_origin_id       = "${var.project_name}-alb-origin"
    allowed_methods        = var.allowed_methods
    cached_methods         = var.cached_methods
    compress               = var.compress
    viewer_protocol_policy = var.viewer_protocol_policy

    # AWS Managed CachingDisabled Policy ID (same across all regions)
    cache_policy_id          = "4135ea2d-6df8-44a3-9df3-4b5a84be39ad"
    origin_request_policy_id = aws_cloudfront_origin_request_policy.include_cloudfront_headers.id
  }

  restrictions {
    geo_restriction {
      restriction_type = "none"
    }
  }

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

  # Ensure policy exists before CF validates behavior
  depends_on = [
    aws_cloudfront_origin_request_policy.include_cloudfront_headers
  ]
}
