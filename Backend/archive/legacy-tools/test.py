import re

content = "public Guid CategoryId { get; set; }"
print(re.sub(r'\bGuid\s+CategoryId\b', 'int CategoryId', content))

content2 = """        var template = new GreetingTemplate
        {
            Id = Guid.NewGuid(),
            CategoryId = request.CategoryId,"""
print(re.sub(r'^\s*Id\s*=\s*Guid\.NewGuid\(\),\s*\n', '', content2, flags=re.MULTILINE))
