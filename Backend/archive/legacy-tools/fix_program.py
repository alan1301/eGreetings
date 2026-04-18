import os
import re

dir_path = "services"

for root, dirs, files in os.walk(dir_path):
    if "Program.cs" in files:
        filepath = os.path.join(root, "Program.cs")
        with open(filepath, "r", encoding="utf-8") as f:
            content = f.read()
        
        orig_content = content
        
        # 1. UseNpgsql -> UseSqlServer
        content = re.sub(r'\.UseNpgsql\(', '.UseSqlServer(', content)
        
        # 2. Connection string keys: egreetings_feedback_db -> Default
        content = re.sub(r'GetConnectionString\("egreetings_[^"]+"\)', 'GetConnectionString("Default")', content)
        
        # 3. ISystemClock clock
        content = re.sub(r',\s*Microsoft\.AspNetCore\.Authentication\.ISystemClock\s+clock\)', ')', content)
        content = re.sub(r':\s*base\(options,\s*logger,\s*encoder,\s*clock\)', ': base(options, logger, encoder)', content)
        
        # 4. RabbitMQ warnings (adding !)
        content = re.sub(r'\.Username\(rabbitMqConfig\["Username"\]\)', '.Username(rabbitMqConfig["Username"]!)', content)
        content = re.sub(r'\.Password\(rabbitMqConfig\["Password"\]\)', '.Password(rabbitMqConfig["Password"]!)', content)
        
        # 5. Fix AddHealthChecks() to use AddServiceHealthChecks(builder.Configuration)
        # Note: some services use AddDbContextCheck<> and AddRabbitMQ(), they span multiple lines.
        health_check_pattern = r'builder\.Services\.AddHealthChecks\(\)[\s\r\n\.]*[a-zA-Z<>_0-9\(\)]*;'
        # A more robust health check replacement
        content = re.sub(r'builder\.Services\.AddHealthChecks\(\)[\s\S]*?;', 'builder.Services.AddServiceHealthChecks(builder.Configuration);', content)
        
        if 'AddServiceHealthChecks' in content and 'using EGreetings.Shared.Infrastructure.Extensions;' not in content:
            content = 'using EGreetings.Shared.Infrastructure.Extensions;\n' + content
        
        if content != orig_content:
            with open(filepath, "w", encoding="utf-8") as f:
                f.write(content)
            print(f"Fixed {filepath}")
