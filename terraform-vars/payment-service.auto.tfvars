services = {
  payment = {
    alb_target_group_port     = 5006
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
    alb_listener_rule_priority = 13
    alb_listener_rule_conditions = [
      {
        path_pattern = {
          values = ["/api/payment/*"]
        }
      }
    ]
    ecs_service_connect_dns_name       = "payment-service"
    ecs_service_connect_discovery_name = "payment-service"
    ecs_service_connect_port_name      = "payment"
    ecs_container_name_suffix          = "microservice"
    ecs_container_image_repository_url = "936910352865.dkr.ecr.us-east-1.amazonaws.com/learnmate-infrastructure-chooy5704-ecr"
    ecs_container_image_tag            = "Payment.Microservice-latest"
    ecs_container_cpu                  = 461
    ecs_container_memory               = 230
    ecs_container_essential            = true
    ecs_container_port_mappings = [
      {
        container_port = 5006
        host_port      = 0
        protocol       = "tcp"
        name           = "payment"
      }
    ]

    ecs_environment_variables = [
      { name = "ASPNETCORE_ENVIRONMENT", value = "Production" },
      { name = "DATABASE_HOST", value = "pg-2-database25812.g.aivencloud.com" },
      { name = "DATABASE_PORT", value = "19217" },
      { name = "DATABASE_NAME", value = "paymentdb" },
      { name = "DATABASE_USERNAME", value = "avnadmin" },
      { name = "DATABASE_PASSWORD", value = "AVNS_vsIotPLRrxJUhcJlM0m" },
      { name = "DATABASE_SSLMODE", value = "Require" },
      { name = "ASPNETCORE_URLS", value = "http://+:5006" },
      { name = "USE_HTTPS", value = "true" },
      { name = "RABBITMQ_HOST", value = "rabbitmq" },
      { name = "RABBITMQ_PORT", value = "5672" },
      { name = "RABBITMQ_USERNAME", value = "rabbitmq" },
      { name = "RABBITMQ_PASSWORD", value = "0Kg04Rq08!" },
      { name = "REDIS_HOST", value = "redis" },
      { name = "REDIS_PASSWORD", value = "0Kg04Rs05!" },
      { name = "REDIS_PORT", value = "6379" },
      { name = "Jwt__SecretKey", value = "YourSuperSecretKeyThatIsAtLeast32CharactersLong!@#$%^&*()" },
      { name = "Jwt__Issuer", value = "UserMicroservice" },
      { name = "Jwt__Audience", value = "MicroservicesApp" },
      { name = "Jwt__ExpirationMinutes", value = "60" },
      # ZaloPay Configuration
      { name = "ZaloPay__AppId", value = "2554" },
      { name = "ZaloPay__Key1", value = "sdngKKJmqEMzvh5QQcdD2A9XBSKUNaYn" },
      { name = "ZaloPay__Key2", value = "trMrHtvjo6myautxDUiAcYsVtaeQ8nhf" },
      { name = "ZaloPay__BaseUrl", value = "https://sb-openapi.zalopay.vn" },
      { name = "MoMo__PartnerCode", value = "MOMONPMB20210629" },
      { name = "MoMo__AccessKey", value = "Q2XhhSdgpKUlQ4Ky" },
      { name = "MoMo__SecretKey", value = "k6B53GQKSjktZGJBK2MyrDa7w9S6RyCf" },
      { name = "MoMo__BaseUrl", value = "https://test-payment.momo.vn" }
    ]

    ecs_container_health_check = {
      command     = ["CMD-SHELL", "curl -f http://localhost:5006/health || exit 1"]
      interval    = 30
      timeout     = 5
      retries     = 3
      startPeriod = 10
    }
    depends_on = ["rabbitmq", "redis"]
  }
}
