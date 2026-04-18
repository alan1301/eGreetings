import os

filepath = "services/admin/EGreetings.Admin.API/Program.cs"
with open(filepath, "r", encoding="utf-8") as file:
    content = file.read()

if 'using EGreetings.Shared.Infrastructure.Middleware;' not in content:
    content = 'using EGreetings.Shared.Infrastructure.Middleware;\n' + content

content = content.replace('return AuthenticateResult', 'return Microsoft.AspNetCore.Authentication.AuthenticateResult')
content = content.replace('Task.FromResult(AuthenticateResult', 'Task.FromResult(Microsoft.AspNetCore.Authentication.AuthenticateResult')

with open(filepath, "w", encoding="utf-8") as file:
    file.write(content)
print("Fixed Admin API Program.cs")
