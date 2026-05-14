import { ChangeDetectionStrategy, Component, model } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-personalize-text-editor',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="space-y-4">
      <div>
        <label class="form-label" for="p-heading">Heading (Recipient Name)</label>
        <input id="p-heading" type="text"
               [value]="heading()" (input)="heading.set($any($event.target).value)"
               class="input-field font-headline text-lg" placeholder="John Doe...">
      </div>
      <div>
        <label class="form-label" for="p-msg">Message (max 500 characters)</label>
        <textarea id="p-msg"
                  [value]="message()" (input)="message.set($any($event.target).value)"
                  class="input-field font-headline italic text-base leading-relaxed resize-none"
                  rows="5" maxlength="500" placeholder="Write your wishes..."></textarea>
        <p class="text-xs text-on-surface-variant mt-1 text-right"
           [class.text-error]="message().length >= 450">
          {{ message().length }}/500
        </p>
      </div>
    </div>
  `
})
export class PersonalizeTextEditorComponent {
  heading = model<string>('');
  message = model<string>('');
}
