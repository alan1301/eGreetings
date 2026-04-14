#!/bin/bash
echo "🚀 Đang khởi động hệ thống EGreetings Microservices..."

# 1. Đảm bảo Infrastructure đang chạy qua Docker
echo "Mở RabbitMQ & Redis..."
docker compose up -d rabbitmq redis || docker-compose up -d rabbitmq redis

# Hàm hỗ trợ chạy app dưới dạng background
start_service() {
    local folder=$1
    local port=$2
    local name=$3
    echo -e "\nBật [$name] ở port $port..."
    cd $folder
    nohup dotnet run --urls "http://localhost:$port" > /dev/null 2>&1 &
    cd - > /dev/null
}

echo "Bắt đầu chạy code..."
start_service "src/EGreetings.API" "5008" "Monolith API"
start_service "services/identity/EGreetings.Identity.API" "5001" "Identity Service"
start_service "services/greeting/EGreetings.Greeting.API" "5002" "Greeting Service"
start_service "services/subscription/EGreetings.Subscription.API" "5003" "Subscription Service"
start_service "services/user/EGreetings.User.API" "5004" "User Service"
start_service "services/notification/EGreetings.Notification.API" "5005" "Notification Service"
start_service "services/admin/EGreetings.Admin.API" "5006" "Admin Service"
start_service "services/feedback/EGreetings.Feedback.API" "5007" "Feedback Service"

# Cuối cùng là Gateway
start_service "gateway/EGreetings.Gateway" "5000" "API Gateway"

echo -e "\n✅ Khởi động thành công! Vui lòng chờ vài giây để code biên dịch và chạy..."
echo "👉 Truy cập Gateway: http://localhost:5000/health"
echo "👉 Truy cập Identity: http://localhost:5001/health"

echo -e "\n(Lưu ý: Để tắt toàn bộ, dùng lệnh: pkill -f 'dotnet run')"
