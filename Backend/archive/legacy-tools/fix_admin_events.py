import os

dir_path = "services/admin/EGreetings.Admin.Infrastructure/Messaging/Consumers"
for f in os.listdir(dir_path):
    if f.endswith(".cs"):
        filepath = os.path.join(dir_path, f)
        with open(filepath, "r", encoding="utf-8") as file:
            content = file.read()
        
        if 'using EGreetings.Shared.Events;' in content:
            content = content.replace('using EGreetings.Shared.Events;', 'using EGreetings.Shared.Contracts.Events;')
            with open(filepath, "w", encoding="utf-8") as file:
                file.write(content)
            print(f"Fixed {filepath}")
