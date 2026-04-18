import os
import re

dir_path = "services"
for root, dirs, files in os.walk(dir_path):
    for f in files:
        if f.endswith(".Application.csproj"):
            filepath = os.path.join(root, f)
            with open(filepath, "r", encoding="utf-8") as file:
                content = file.read()
            
            if 'Microsoft.EntityFrameworkCore' not in content:
                content = content.replace('</Project>', '  <ItemGroup>\n    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />\n  </ItemGroup>\n</Project>')
                with open(filepath, "w", encoding="utf-8") as file:
                    file.write(content)
                print(f"Added EF Core to {filepath}")

# Fix IFeedbackDbContext
feedback_db_ctx = "services/feedback/EGreetings.Feedback.Application/Common/Interfaces/IFeedbackDbContext.cs"
if os.path.exists(feedback_db_ctx):
    with open(feedback_db_ctx, "r") as f:
        c = f.read()
    c = c.replace("DbSet<Feedback>", "DbSet<Domain.Entities.Feedback>")
    with open(feedback_db_ctx, "w") as f:
        f.write(c)
    print("Fixed IFeedbackDbContext namespace collision.")
