services = {
  apigateway = {
    alb_target_group_port     = 8080
    alb_target_group_protocol = "HTTP"
    alb_target_group_type     = "ip"
    alb_health_check = {
      enabled             = true
      path                = "/health"
      port                = "traffic-port"
      protocol            = "HTTP"
      matcher             = "200"
      interval            = 30
      timeout             = 5
      healthy_threshold   = 2
      unhealthy_threshold = 3
    }
    alb_listener_rule_priority   = 10
    alb_listener_rule_conditions = []

    ecs_service_connect_dns_name       = "api-gateway"
    ecs_service_connect_discovery_name = "api-gateway"
    ecs_service_connect_port_name      = "apigateway"
    ecs_container_name_suffix          = "apigateway"
    ecs_container_image_repository_url = "936910352865.dkr.ecr.us-east-1.amazonaws.com/learnmate-infrastructure-chooy5704-ecr"
    ecs_container_image_tag            = "ApiGateway-latest"
    ecs_container_cpu                  = 120
    ecs_container_memory               = 120
    ecs_container_essential            = true
    ecs_container_port_mappings = [
      {
        container_port = 8080
        host_port      = 0
        protocol       = "tcp"
        name           = "apigateway"
      }
    ]

    ecs_environment_variables = [
      { name = "ENABLE_SWAGGER_UI", value = "true" },
      { name = "ASPNETCORE_ENVIRONMENT", value = "Production" },
      { name = "ASPNETCORE_URLS", value = "http://+:8080" },
      { name = "USER_MICROSERVICE_HOST", value = "user-service" },
      { name = "USER_MICROSERVICE_PORT", value = "5002" },
      { name = "AI_MICROSERVICE_HOST", value = "ai-service" },
      { name = "AI_MICROSERVICE_PORT", value = "5003" },
      { name = "BOOK_MICROSERVICE_HOST", value = "book-service" },
      { name = "BOOK_MICROSERVICE_PORT", value = "5004" },
      { name = "SUBSCRIPTION_MICROSERVICE_HOST", value = "subscription-service" },
      { name = "SUBSCRIPTION_MICROSERVICE_PORT", value = "5005" },
      { name = "PAYMENT_MICROSERVICE_HOST", value = "payment-service" },
      { name = "PAYMENT_MICROSERVICE_PORT", value = "5006" },
      { name = "Jwt__SecretKey", value = "YourSuperSecretKeyThatIsAtLeast32CharactersLong!@#$%^&*()" },
      { name = "Jwt__Issuer", value = "UserMicroservice" },
      { name = "Jwt__Audience", value = "MicroservicesApp" },
      { name = "Jwt__ExpirationMinutes", value = "60" }
    ]

    ecs_container_health_check = {
      command     = ["CMD-SHELL", "curl -f http://localhost:8080/health || exit 1"]
      interval    = 30
      timeout     = 5
      retries     = 3
      startPeriod = 10
    }
    depends_on = ["user-microservice"]
  }
}
