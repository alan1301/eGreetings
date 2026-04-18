import os
import re

dir_path = "services"
for root, dirs, files in os.walk(dir_path):
    for f in files:
        if f.endswith(".cs"):
            filepath = os.path.join(root, f)
            with open(filepath, "r", encoding="utf-8") as file:
                content = file.read()
            
            orig = content
            content = re.sub(r',\s*Guid>', ', int>', content)
            
            if orig != content:
                with open(filepath, "w", encoding="utf-8") as file:
                    file.write(content)
                print(f"Fixed Guid in {filepath}")
