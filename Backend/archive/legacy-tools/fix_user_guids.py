import os
import re

dir_path = "services/user"
for root, dirs, files in os.walk(dir_path):
    if "Migrations" in root:
        continue
    for f in files:
        if f.endswith(".cs"):
            filepath = os.path.join(root, f)
            with open(filepath, "r", encoding="utf-8") as file:
                content = file.read()
            
            orig = content
            content = re.sub(r'\bGuid\s+userId\b', 'int userId', content)
            content = re.sub(r'\bGuid\?\s+userId\b', 'int? userId', content)
            content = re.sub(r'\bGuid\.Parse', 'int.Parse', content)
            content = re.sub(r'\bGuid\.TryParse', 'int.TryParse', content)
            content = re.sub(r'\bGuid\.NewGuid\(\)', '0', content) # for dummy replacements where needed, or delete entirely.
            # Replace userId != currentUserId type of Guid vs int. Let's see if there is any `Guid` left.
            
            # Remove Id = Guid.NewGuid() assignments
            content = re.sub(r'Id\s*=\s*Guid\.NewGuid\(\),?\s*\n', '', content)
            
            if orig != content:
                with open(filepath, "w", encoding="utf-8") as file:
                    file.write(content)
