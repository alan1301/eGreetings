import os

dir_path = "services"
for root, dirs, files in os.walk(dir_path):
    for f in files:
        if f.endswith(".API.csproj"):
            filepath = os.path.join(root, f)
            with open(filepath, "r", encoding="utf-8") as file:
                content = file.read()
            
            ref = '    <ProjectReference Include="../../../shared/EGreetings.Shared.Infrastructure/EGreetings.Shared.Infrastructure.csproj" />\n'
            
            if 'EGreetings.Shared.Infrastructure.csproj' not in content:
                # Add to the existing ItemGroup with ProjectReference
                if '<ProjectReference' in content:
                    content = content.replace('</ItemGroup>', f'{ref}  </ItemGroup>', 1)
                else:
                    content = content.replace('</Project>', f'  <ItemGroup>\n{ref}  </ItemGroup>\n</Project>')
                
                with open(filepath, "w", encoding="utf-8") as file:
                    file.write(content)
                print(f"Added Shared.Infrastructure ref to {filepath}")
