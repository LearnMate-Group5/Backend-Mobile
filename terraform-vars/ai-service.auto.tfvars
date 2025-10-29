services = {
  ai = {
    alb_target_group_port     = 5003
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
    alb_listener_rule_priority = 11
    alb_listener_rule_conditions = [
      {
        path_pattern = {
          values = ["/api/ai/*"]
        }
      }
    ]
    ecs_service_connect_dns_name       = "ai-service"
    ecs_service_connect_discovery_name = "ai-service"
    ecs_service_connect_port_name      = "ai"
    ecs_container_name_suffix          = "microservice"
    ecs_container_image_repository_url = "936910352865.dkr.ecr.us-east-1.amazonaws.com/learnmate-infrastructure-chooy5704-ecr"
    ecs_container_image_tag            = "Ai.Microservice-latest"
    ecs_container_cpu                  = 120
    ecs_container_memory               = 120
    ecs_container_essential            = true
    ecs_container_port_mappings = [
      {
        container_port = 5003
        host_port      = 0
        protocol       = "tcp"
        name           = "ai"
      }
    ]

    ecs_environment_variables = [
      { name = "ASPNETCORE_ENVIRONMENT", value = "Production" },
      { name = "ASPNETCORE_URLS", value = "http://+:5003" },
      { name = "AiWebhook__Endpoint", value = "http://n8n:5678/webhook/upload-and-translate" },
      { name = "AiWebhook__TimeoutSeconds", value = "120" },
      { name = "DATABASE_HOST", value = "pg-2-database25812.g.aivencloud.com" },
      { name = "DATABASE_PORT", value = "19217" },
      { name = "DATABASE_NAME", value = "aidb" },
      { name = "DATABASE_USERNAME", value = "avnadmin" },
      { name = "DATABASE_PASSWORD", value = "AVNS_vsIotPLRrxJUhcJlM0m" },
      { name = "DATABASE_SSLMODE", value = "Require" }
    ]

    ecs_container_health_check = {
      command     = ["CMD-SHELL", "curl -f http://localhost:5003/health || exit 1"]
      interval    = 30
      timeout     = 5
      retries     = 3
      startPeriod = 10
    }
    depends_on = ["rabbitmq", "redis"]
  }
}
