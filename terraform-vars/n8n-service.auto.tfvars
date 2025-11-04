services = {
  n8n = {
    alb_target_group_port     = 8088
    alb_target_group_protocol = "HTTP"
    alb_target_group_type     = "ip"
    alb_health_check = {
      enabled             = true
      path                = "/n8n/healthz"
      port                = "traffic-port"
      protocol            = "HTTP"
      matcher             = "200-399"
      interval            = 30
      timeout             = 5
      healthy_threshold   = 2
      unhealthy_threshold = 3
    }
    alb_listener_rule_priority   = 15
    alb_listener_rule_conditions = [
      {
        path_pattern = {
          values = [
            "/n8n",
            "/n8n/*"
          ]
        }
      }
    ]

    ecs_service_connect_dns_name       = "n8n"
    ecs_service_connect_discovery_name = "n8n"
    ecs_service_connect_port_name      = "n8n"
    ecs_container_name_suffix          = "n8n"
    ecs_container_image_repository_url = "docker.n8n.io/n8nio/n8n"
    ecs_container_image_tag            = "latest"
    ecs_container_cpu                  = 256
    ecs_container_memory               = 400
    ecs_task_memory                    = 800
    ecs_container_essential            = true
    ecs_container_port_mappings = [
      {
        container_port = 5678
        host_port      = 0
        protocol       = "tcp"
        name           = "n8n"
      }
    ]

    ecs_environment_variables = [
      { name = "N8N_HOST", value = "0.0.0.0" },
      { name = "N8N_PORT", value = "5678" },
      { name = "N8N_PROTOCOL", value = "http" },
      { name = "N8N_SECURE_COOKIE", value = "false" },
      { name = "N8N_PATH", value = "/n8n/" },
      { name = "GENERIC_TIMEZONE", value = "Asia/Ho_Chi_Minh" },
      { name = "TZ", value = "Asia/Ho_Chi_Minh" },
      { name = "N8N_ENFORCE_SETTINGS_FILE_PERMISSIONS", value = "true" },
      { name = "N8N_DIAGNOSTICS_ENABLED", value = "false" },
      { name = "N8N_VERSION_NOTIFICATIONS_ENABLED", value = "false" },
      { name = "N8N_TEMPLATES_ENABLED", value = "false" },
      { name = "N8N_METRICS", value = "true" },
      { name = "QUEUE_HEALTH_CHECK_ACTIVE", value = "true" },
      { name = "N8N_RUNNERS_ENABLED", value = "true" },
      { name = "N8N_BLOCK_ENV_ACCESS_IN_NODE", value = "false" },
      { name = "N8N_GIT_NODE_DISABLE_BARE_REPOS", value = "true" },
      { name = "NODE_OPTIONS", value = "--max-old-space-size=352" },
      # PostgreSQL Database Configuration
      { name = "DB_TYPE", value = "postgresdb" },
      { name = "DB_POSTGRESDB_HOST", value = "pg-2-database25812.g.aivencloud.com" },
      { name = "DB_POSTGRESDB_PORT", value = "19217" },
      { name = "DB_POSTGRESDB_DATABASE", value = "aidb" },
      { name = "DB_POSTGRESDB_USER", value = "avnadmin" },
      { name = "DB_POSTGRESDB_PASSWORD", value = "AVNS_vsIotPLRrxJUhcJlM0m" },
      { name = "DB_POSTGRESDB_SSL_ENABLED", value = "true" },
      { name = "DB_POSTGRESDB_SSL_REJECT_UNAUTHORIZED", value = "false" }
    ]

    ecs_container_health_check = {
      command     = ["CMD-SHELL", "node -e \"http=require('http');http.get('http://localhost:5678/healthz',res=>{process.exit((res.statusCode>=200&&res.statusCode<400)?0:1)}).on('error',()=>process.exit(1))\""]
      interval    = 30
      timeout     = 5
      retries     = 3
      startPeriod = 30
    }
    depends_on = []
  }
}
