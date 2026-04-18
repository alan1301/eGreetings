import os

dir_path = "services/identity"
for root, dirs, files in os.walk(dir_path):
    for f in files:
        if f.endswith(".cs"):
            filepath = os.path.join(root, f)
            with open(filepath, "r", encoding="utf-8") as file:
                content = file.read()
            
            if 'using EGreetings.Shared.Application;' in content:
                content = content.replace('using EGreetings.Shared.Application;', 'using EGreetings.Shared.Domain;')
                with open(filepath, "w", encoding="utf-8") as file:
                    file.write(content)
                print(f"Fixed {filepath}")
