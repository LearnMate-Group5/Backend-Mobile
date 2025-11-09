variable "project_name" {
  description = "Base name applied to CloudFront-related resources."
  type        = string
}

variable "alb_dns_name" {
  description = "DNS name of the Application Load Balancer to use as CloudFront origin."
  type        = string
}

variable "alb_id" {
  description = "ID of the Application Load Balancer for origin identification."
  type        = string
}

variable "enable_caching" {
  description = "Whether to enable CloudFront caching. Set to false to pass all requests directly to ALB."
  type        = bool
  default     = false
}

variable "price_class" {
  description = "CloudFront price class. Options: PriceClass_All, PriceClass_200, PriceClass_100"
  type        = string
  default     = "PriceClass_100" # Use only North America and Europe edge locations (cheapest)
}

variable "viewer_protocol_policy" {
  description = "How CloudFront handles HTTP/HTTPS. Options: allow-all, redirect-to-https, https-only"
  type        = string
  default     = "redirect-to-https"
}

variable "viewer_min_protocol_version" {
  description = "Minimum TLS version CloudFront accepts from viewers (e.g., TLSv1, TLSv1.1, TLSv1.2_2018, TLSv1.2_2021)."
  type        = string
  default     = "TLSv1.2_2018"
}

variable "min_ttl" {
  description = "Minimum amount of time objects stay in CloudFront cache (seconds). 0 = no caching"
  type        = number
  default     = 0
}

variable "default_ttl" {
  description = "Default amount of time objects stay in CloudFront cache (seconds). 0 = no caching"
  type        = number
  default     = 0
}

variable "max_ttl" {
  description = "Maximum amount of time objects stay in CloudFront cache (seconds). 0 = no caching"
  type        = number
  default     = 0
}

variable "allowed_methods" {
  description = "HTTP methods CloudFront processes and forwards to ALB."
  type        = list(string)
  default     = ["DELETE", "GET", "HEAD", "OPTIONS", "PATCH", "POST", "PUT"]
}

variable "cached_methods" {
  description = "HTTP methods for which CloudFront caches responses."
  type        = list(string)
  default     = ["GET", "HEAD", "OPTIONS"]
}

variable "compress" {
  description = "Whether CloudFront automatically compresses content."
  type        = bool
  default     = true
}

variable "forward_cookies" {
  description = "How to forward cookies to origin. Options: none, whitelist, all"
  type        = string
  default     = "all"
}

variable "forward_query_string" {
  description = "Whether to forward query strings to the origin."
  type        = bool
  default     = true
}

variable "forward_headers" {
  description = "Headers to forward to the origin. Use ['*'] to forward all headers (disables caching)."
  type        = list(string)
  default     = ["*"] # Forward all headers to support API functionality
}
