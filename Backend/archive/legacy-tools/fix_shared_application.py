import os

dir_path = "services/identity"
for root, dirs, files in os.walk(dir_path):
    for f in files:
        if f.endswith(".cs"):
            filepath = os.path.join(root, f)
            with open(filepath, "r", encoding="utf-8") as file:
                content = file.read()
            
            if 'EGreetings.Shared.Application.Models' in content:
                content = content.replace('EGreetings.Shared.Application.Models', 'EGreetings.Shared.Contracts.Models')
                with open(filepath, "w", encoding="utf-8") as file:
                    file.write(content)
                print(f"Fixed {filepath}")
