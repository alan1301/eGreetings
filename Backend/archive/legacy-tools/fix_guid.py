import os
import re

dir_path = "services/greeting"

# Replacements list (old, new)
# Regexes to be safe
replacements = [
    (r'\bGuid\?\s+UserId\b', r'int? UserId'),
    (r'\bGuid\s+UserId\b', r'int UserId'),
    (r'\bGuid\?\s+userId\b', r'int? userId'),
    (r'\bGuid\s+TemplateId\b', r'int TemplateId'),
    (r'\bGuid\?\s+CategoryId\b', r'int? CategoryId'),
    (r'\bGuid\s+CategoryId\b', r'int CategoryId'),
    (r'\bGuid\s+id\b', r'int id'),
    (r'\bGuid\s+Id\b', r'int Id'),
    (r'\bGuid\s+GreetingId\b', r'int GreetingId'),
    (r'Guid\.Parse\(', r'int.Parse('),
    (r'Guid\.TryParse\(', r'int.TryParse('),
    (r'^\s*Id\s*=\s*Guid\.NewGuid\(\),\s*\n', r'')
]

for root, dirs, files in os.walk(dir_path):
    for filename in files:
        if filename.endswith(".cs"):
            filepath = os.path.join(root, filename)
            with open(filepath, "r", encoding="utf-8") as f:
                content = f.read()
            
            orig_content = content
            for old, new in replacements:
                content = re.sub(old, new, content, flags=re.MULTILINE)
            
            if content != orig_content:
                with open(filepath, "w", encoding="utf-8") as f:
                    f.write(content)
                print(f"Fixed {filepath}")
