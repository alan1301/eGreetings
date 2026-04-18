import os
import re

dir_path = "services"

# Comprehensive replacements
replacements = [
    # General ID fields in Classes/Records
    (r'\bGuid\?\s+([A-Za-z]+Id)\b', r'int? \1'),
    (r'\bGuid\s+([A-Za-z]+Id)\b', r'int \1'),
    
    # "Id" field
    (r'\bGuid\s+id\b', r'int id'),
    (r'\bGuid\s+Id\b', r'int Id'),
    
    # Types parameters like <Guid> -> <int>
    (r'<Guid>', r'<int>'),
    (r'<Guid,', r'<int,'),
    
    # Parse methods
    (r'Guid\.Parse\(', r'int.Parse('),
    (r'Guid\.TryParse\(', r'int.TryParse('),
    
    # Guid.NewGuid() assigned to Id/UserId/etc
    (r'^\s*[A-Za-z]*Id\s*=\s*Guid\.NewGuid\(\),\s*\n', r''),
]

for root, dirs, files in os.walk(dir_path):
    # Don't touch migration files since changing types in migrations usually requires dropping DB.
    # But this is a greenfield-ish state, Migration files might break the build if we change entities but not migrations.
    # We should probably delete old migrations and recreate them later if needed.
    if "Migrations" in root:
        continue
        
    for filename in files:
        if filename.endswith(".cs"):
            filepath = os.path.join(root, filename)
            with open(filepath, "r", encoding="utf-8") as f:
                content = f.read()
            
            orig_content = content
            for old, new in replacements:
                content = re.sub(old, new, content, flags=re.MULTILINE)
            
            # Additional cleanup for 'Id = Guid.NewGuid();' inside constructors
            content = re.sub(r'Id\s*=\s*Guid\.NewGuid\(\);\s*', '', content)
            
            # Avoid replacing Guid.NewGuid().ToString() for Tokens (which is fine)
            # The above regexes shouldn't touch `var token = Guid.NewGuid().ToString()`
            
            if content != orig_content:
                with open(filepath, "w", encoding="utf-8") as f:
                    f.write(content)
                print(f"Fixed Guids in {filepath}")
