import os

filepath = "services/notification/EGreetings.Notification.API/Program.cs"
with open(filepath, "r", encoding="utf-8") as file:
    content = file.read()

if 'using EGreetings.Shared.Infrastructure.Middleware;' not in content:
    content = 'using EGreetings.Shared.Infrastructure.Middleware;\n' + content

with open(filepath, "w", encoding="utf-8") as file:
    file.write(content)
print("Fixed Notification API Program.cs")
