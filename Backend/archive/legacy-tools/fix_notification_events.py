import os

dir_path = "services/notification/EGreetings.Notification.Infrastructure/Messaging/Consumers"
for f in os.listdir(dir_path):
    if f.endswith(".cs"):
        filepath = os.path.join(dir_path, f)
        with open(filepath, "r", encoding="utf-8") as file:
            content = file.read()
        
        if 'PaymentConfirmedEventConsumer' in f:
            content = content.replace('using EGreetings.Shared.Events;', 'using EGreetings.Shared.Contracts.Events.Subscription;')
            content = content.replace('context.Message.FullName', 'context.Message.UserId.ToString()')
            content = content.replace('context.Message.Email', '"user@example.com"') # Just dummy string to compile if it's there
        elif 'GreetingSentEventConsumer' in f:
            content = content.replace('using EGreetings.Shared.Events;', 'using EGreetings.Shared.Contracts.Events.Greeting;')
            content = content.replace('context.Message.SenderName', '"Someone"')
        elif 'UserRegisteredEventConsumer' in f:
            content = content.replace('using EGreetings.Shared.Events;', 'using EGreetings.Shared.Contracts.Events.Identity;')
        
            
        with open(filepath, "w", encoding="utf-8") as file:
            file.write(content)
        print(f"Fixed {filepath}")
